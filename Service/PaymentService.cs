using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using Common.Constants;
using Common.Utils;
using DTOs.Response.Accounts;
using DTOs.Response.Courses;
using Microsoft.AspNetCore.Http;
using Service.VnPay;
using Repository.IRepositories;
using BusinessObject.Model;
using Microsoft.AspNetCore.Identity;

namespace Service
{
    public class PaymentService
    {
        private readonly VnPayApiService _apiService;
        private readonly ICoursekeyRepository _coursekeyRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICourseRepository _courseRepository;

        public PaymentService(VnPayApiService apiService, ICoursekeyRepository coursekeyRepository, IAccountRepository accountRepository, UserManager<Account> userManager, IPaymentRepository paymentRepository, ICourseRepository courseRepository)
        {
            _apiService = apiService;
            _coursekeyRepository = coursekeyRepository;
            _accountRepository = accountRepository;
            _userManager = userManager;
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
        }

        public async Task<BaseResponse<PaymentResponse>> CreatePaymentUrlAsync(HttpContext context, PaymentRequest req)
        {
                var user = context.User;
                var parentId = user.FindFirst("AccountID")?.Value;
                var parentRoles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList();
                
                if (string.IsNullOrEmpty(parentId))
                {
                    throw new Exception("Không tìm thấy thông tin người dùng trong token!");
                }

                if (!parentRoles.Contains("Parent"))
                {
                    // Nếu không có role Parent, thử tạo lại token với role đúng
                    var userAccount = await _accountRepository.GetByStringId(parentId);
                    if (userAccount != null)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(userAccount);
                        if (!currentRoles.Contains("Parent"))
                        {
                            await _userManager.AddToRoleAsync(userAccount, "Parent");
                        }
                    }
                    throw new Exception($"Phải đăng nhập bằng tài khoản phụ huynh để thanh toán khóa học! Current roles: {string.Join(", ", parentRoles)}. Vui lòng đăng nhập lại để nhận token mới.");
                }

                var course = await _courseRepository.GetByIdAsync(req.CourseId);

                // Tạo transaction reference với thông tin course và parent
                var txnRef = $"{req.CourseId}_{parentId}_{DateTime.Now.Ticks}";

                var request = VnPayApiRequest.Builder()
                    .BaseUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html")
                    .AddParameter("vnp_Version", "2.1.0")
                    .AddParameter("vnp_Command", "pay")
                    .AddParameter("vnp_TmnCode", CommonUtils.GetApiKey("VNP_TMNCODE"))
                    .AddParameter("vnp_Amount", (course.Price * 100).ToString())
                    .AddParameter("vnp_CurrCode", "VND")
                    .AddParameter("vnp_TxnRef", txnRef)
                    .AddParameter("vnp_OrderInfo", $"Thanh toan khoa hoc {req.CourseId} cho Parent: {parentId}")
                    .AddParameter("vnp_OrderType", "other")
                    .AddParameter("vnp_Locale", "vn")
                    .AddParameter("vnp_ReturnUrl", CommonUtils.GetApiKey("VNP_RETURN_URL"))
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

        public async Task<BaseResponse<string>> HandlePaymentSuccessAsync(Dictionary<string, string> request)
        {
                // Kiểm tra response code từ VnPay
                var responseCode = request.GetValueOrDefault("vnp_ResponseCode");

                if (responseCode != "00")
                {
                    throw new Exception($"Thanh toán thất bại. Mã lỗi: {responseCode}");
                }

                // Parse thông tin từ txnRef: CourseId_ParentId_Timestamp
                var txnRef = request.GetValueOrDefault("vnp_TxnRef");

                var txnRefParts = txnRef?.Split('_');
                if (txnRefParts == null || txnRefParts.Length < 3)
                {
                    throw new Exception("Thông tin giao dịch không hợp lệ");
                }

                var courseId = int.Parse(txnRefParts[0]);
                var parentId = txnRefParts[1];

                var transaction = new Transactionhistory
                {
                    CourseId = courseId,
                    ParentId = parentId,
                    Amount = int.Parse(request.GetValueOrDefault("vnp_Amount")),
                    VnpTxnRef = txnRef,
                    VnpTransactionDate = request.GetValueOrDefault("vnp_TransactionDate"),
                };

                await _paymentRepository.AddAsync(transaction);

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

        public async Task<List<CourseKeyResponse>> GetAllCourseKeysAsync()
        {
            try
            {
                var courseKeys = await _coursekeyRepository.GetAllCourseKeysAsync();

                var response = courseKeys.Select(k => new CourseKeyResponse
                {
                    KeyValue = k.KeyValue,
                    CourseId = k.CourseId,
                    StudentId = k.StudentId,
                    CreatedAt = k.CreatedAt,
                    Status = k.Status
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                return new List<CourseKeyResponse>();
            }
        }

        public async Task<List<CourseKeyResponse>> GetFilteredCourseKeysAsync(string? status = null, string? parentId = null, int? courseId = null)
        {
            try
            {
                var courseKeys = await _coursekeyRepository.GetAllCourseKeysAsync(status, parentId, courseId);

                var response = courseKeys.Select(k => new CourseKeyResponse
                {
                    KeyValue = k.KeyValue,
                    CourseId = k.CourseId,
                    StudentId = k.StudentId,
                    CreatedAt = k.CreatedAt,
                    Status = k.Status
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                return new List<CourseKeyResponse>();
            }
        }

        public async Task<List<CourseKeyResponse>> GetCourseKeysByParentAsync(string parentId)
        {
            try
            {
                var courseKeys = await _coursekeyRepository.GetByParentIdAsync(parentId);

                var response = courseKeys.Select(k => new CourseKeyResponse
                {
                    KeyValue = k.KeyValue,
                    CourseId = k.CourseId,
                    StudentId = k.StudentId,
                    CreatedAt = k.CreatedAt,
                    Status = k.Status
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                return new List<CourseKeyResponse>();
            }
        }

        public async Task<VnPayQueryApiResponse> GetVnPayTransactionDetail(VnPayQueryApiRequest request, HttpContext context)
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

        private async Task SendKeyToParentAsync(string parentId, string keyValue, int courseId)
        {
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
    }
}
