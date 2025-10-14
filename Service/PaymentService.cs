using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using Common.Constants;
using Common.Utils;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Http;
using Service.VnPay;
using Repository.IRepositories;
using BusinessObject.Model;

namespace Service
{
    public class PaymentService
    {
        private readonly VnPayApiService _apiService;
        private readonly ICoursekeyRepository _coursekeyRepository;
        private readonly IAccountRepository _accountRepository;

        public PaymentService(VnPayApiService apiService, ICoursekeyRepository coursekeyRepository, IAccountRepository accountRepository)
        {
            _apiService = apiService;
            _coursekeyRepository = coursekeyRepository;
            _accountRepository = accountRepository;
        }

        public async Task<BaseResponse<PaymentResponse>> CreatePaymentUrlAsync(HttpContext context, PaymentRequest req)
        {
            try
            {
                var user = context.User;
                var parentId = user.FindFirst("AccountID")?.Value;
                var parentRoles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList();
                if (string.IsNullOrEmpty(parentId) || !parentRoles.Contains("Parent"))
                {
                    throw new Exception("Phải đăng nhập bằng tài khoản phụ huynh để thanh toán khóa học!");
                }

                // Tạo transaction reference với thông tin course và parent
                var txnRef = $"{req.CourseId}_{parentId}_{DateTime.Now.Ticks}";

                var request = VnPayApiRequest.Builder()
                    .BaseUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html")
                    .AddParameter("vnp_Version", "2.1.0")
                    .AddParameter("vnp_Command", "pay")
                    .AddParameter("vnp_TmnCode", CommonUtils.GetApiKey("VNP_TMNCODE"))
                    .AddParameter("vnp_Amount", (req.Amount * 100).ToString())
                    .AddParameter("vnp_CurrCode", "VND")
                    .AddParameter("vnp_TxnRef", txnRef)
                    .AddParameter("vnp_OrderInfo", $"Thanh toan khoa hoc {req.CourseId} cho Parent: {parentId}")
                    .AddParameter("vnp_OrderType", "other")
                    .AddParameter("vnp_Locale", "vn")
                    .AddParameter("vnp_ReturnUrl", "https://yourdomain.com/api/payment/vnpay-callback")
                    .AddParameter("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"))
                    .AddParameter("vnp_IpAddr", CommonUtils.GetIpAddress(context))
                    .HashSecret(CommonUtils.GetApiKey("VNP_HASHSECRET"))
                    .Build();

                var paymentUrl = request.BuildVnPayUrl();
                var paymentQR = VnPayApiRequest.ToQrBase64(paymentUrl);

                return new BaseResponse<PaymentResponse>(
                    "Tạo URL thanh toán thành công. Vui lòng thanh toán để nhận mã khóa học.",
                    StatusCodeEnum.OK_200,
                    new PaymentResponse
                    {
                        PaymentUrl = paymentUrl,
                        PaymentQR = paymentQR
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create vnpay url: {ex.Message}");
            }
        }

        public async Task<BaseResponse<string>> HandlePaymentSuccessAsync(Dictionary<string, string> request)
        {
            try
            {
                // Kiểm tra response code từ VnPay
                var responseCode = request.GetValueOrDefault("vnp_ResponseCode");

                if (responseCode != "00")
                {
                    throw new Exception($"Thanh toán thất bại. Mã lỗi: {responseCode}");
                }

                // Parse thông tin từ txnRef: CourseId_ParentId_Timestamp
                var txnRefParts = request.GetValueOrDefault("vnp_TxnRef")?.Split('_');
                if (txnRefParts == null || txnRefParts.Length < 3)
                {
                    throw new Exception("Thông tin giao dịch không hợp lệ");
                }

                var courseId = int.Parse(txnRefParts[0]);
                var parentId = txnRefParts[1];

                // Tạo CourseKey khi thanh toán thành công
                var uniqueKeyValue = Guid.NewGuid().ToString("N");
                var courseKey = new Coursekey
                {
                    CourseId = courseId,
                    StudentId = null,
                    CreatedBy = parentId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = "available",
                    KeyValue = uniqueKeyValue
                };
                await _coursekeyRepository.AddAsync(courseKey);

                // Gửi key cho Parent
                await SendKeyToParentAsync(parentId, uniqueKeyValue, courseId);

                return new BaseResponse<string>(
                    "Thanh toán thành công! Mã khóa học đã được gửi cho phụ huynh.",
                    StatusCodeEnum.OK_200,
                    uniqueKeyValue
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to handle payment success: {ex.Message}");
            }
        }

        public async Task<VnPayQueryApiResponse> GetVnPayTransactionDetail(VnPayQueryApiRequest request, HttpContext context)
        {
            try
            {
                var body = new VnPayQueryApiBody
                {
                    VnpRequestId = Guid.NewGuid().ToString(),
                    VnpVersion = "2.1.0",
                    VnpCommand = "querydr",
                    VnpTmnCode = CommonUtils.GetApiKey("VNP_TMNCODE"),
                    VnpTxnRef = request.VnpTxnRef,
                    VnpTransactionDate = request.VnpTransactionDate,
                    VnpCreateDate = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    VnpIpAddr = CommonUtils.GetIpAddress(context),
                };

                var data = string.Join("|", new[]
                {
                    body.VnpRequestId,
                    body.VnpVersion,
                    body.VnpCommand,
                    body.VnpTmnCode,
                    body.VnpTxnRef,
                    body.VnpTransactionDate,
                    body.VnpCreateDate,
                    body.VnpIpAddr,
                    body.VnpOrderInfo
                });

                body.VnpSecureHash = CommonUtils.HmacSHA512(data, CommonUtils.GetApiKey("VNP_HASHSECRET"));

                var apiRequest = VnPayApiRequest.Builder()
                    .Body(body)
                    .Build();

                var response = await _apiService.PostAsync<VnPayQueryApiBody, VnPayQueryApiResponse>(apiRequest, body);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to handle payment success: {ex.Message}");
            }
        }

        private async Task SendKeyToParentAsync(string parentId, string keyValue, int courseId)
        {
            try
            {
                // Lấy thông tin Parent
                var parent = await _accountRepository.GetByStringId(parentId);
                if (parent == null)
                {
                    throw new Exception("Không tìm thấy thông tin phụ huynh");
                }

                // TODO: Implement gửi email/SMS cho Parent
                // Hiện tại chỉ log ra console
                Console.WriteLine($"=== THÔNG BÁO CHO PHỤ HUYNH ===");
                Console.WriteLine($"Phụ huynh: {parent.Name} ({parent.Email})");
                Console.WriteLine($"Khóa học ID: {courseId}");
                Console.WriteLine($"Mã khóa học: {keyValue}");
                Console.WriteLine($"Hướng dẫn: Đưa mã này cho học sinh để kích hoạt khóa học");
                Console.WriteLine($"================================");

                // TODO: Gửi email thực tế
                // await _emailService.SendCourseKeyAsync(parent.Email, keyValue, courseId);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không ảnh hưởng đến việc tạo key
                Console.WriteLine($"Warning: Failed to send key to parent: {ex.Message}");
            }
        }
    }
}
