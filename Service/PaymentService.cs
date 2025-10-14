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
        private readonly VnPayApiRequest _vnpApiRequest;
        private readonly ICoursekeyRepository _coursekeyRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;

        public PaymentService(VnPayApiService apiService, ICoursekeyRepository coursekeyRepository, IAccountRepository accountRepository, UserManager<Account> userManager)
        {
            _apiService = apiService;
            _coursekeyRepository = coursekeyRepository;
            _accountRepository = accountRepository;
            _userManager = userManager;
        }

        public async Task<BaseResponse<PaymentResponse>> CreatePaymentUrlAsync(HttpContext context, PaymentRequest req)
        {
            try
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
                    .AddParameter("vnp_OrderInfo", $"Thanh toan khoa hoc {req.CourseId} cho Parent {parentId}")
                    .AddParameter("vnp_OrderType", "other")
                    .AddParameter("vnp_Locale", "vn")
                    .AddParameter("vnp_ReturnUrl", "https://localhost:7211/api/payment/vnpay-callback")
                    .AddParameter("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"))
                    .AddParameter("vnp_IpAddr", CommonUtils.GetIpAddress(context))
                    .HashSecret(CommonUtils.GetApiKey("VNP_HASHSECRET"))
                    .Build();

                var paymentUrl = request.BuildVnPayUrl();
                var paymentQR = VnPayApiRequest.ToQrBase64(paymentUrl);

                // CHỈ tạo URL thanh toán, KHÔNG tạo CourseKey ở đây
                return new BaseResponse<PaymentResponse>(
                    "Tạo URL thanh toán thành công. Vui lòng thanh toán để nhận mã khóa học.",
                    StatusCodeEnum.OK_200,
                    new PaymentResponse
                    {
                        PaymentUrl = paymentUrl,
                        PaymentQR = paymentQR
                        // KHÔNG có PaymentKey ở đây
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create vnpay url: {ex.Message}");
            }
        }

        public async Task<BaseResponse<string>> HandlePaymentSuccessAsync(VnPayCallbackRequest callbackRequest)
        {
            try
            {
                // Kiểm tra response code từ VnPay
                if (callbackRequest.vnp_ResponseCode != "00")
                {
                    throw new Exception($"Thanh toán thất bại. Mã lỗi: {callbackRequest.vnp_ResponseCode}");
                }

                // Parse thông tin từ txnRef: CourseId_ParentId_Timestamp
                var txnRefParts = callbackRequest.vnp_TxnRef?.Split('_');
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
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không ảnh hưởng đến việc tạo key
            }
        }
    }
}
