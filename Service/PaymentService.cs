using Common.Constants;
using Common.Utils;
using Repository.IRepositories;
using BusinessObject.Model;
using BusinessObject.Payload.Request.PayOS;
using BusinessObject.Payload.Response.PayOS;
using Service.PayOS;
using BusinessObject.DTOs.Response;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using BusinessObject.DTOs.Response.Courses;

namespace Service
{
    public class PaymentService
    {
        private readonly ICoursekeyRepository _coursekeyRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly PayOSApiService _payOSApiService;
        private readonly IPackageRepository _packageRepository;

        public PaymentService(
            ICoursekeyRepository coursekeyRepository,
            IAccountRepository accountRepository,
            UserManager<Account> userManager,
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            PayOSApiService payOSApiService,
            IPackageRepository packageRepository)
        {
            _coursekeyRepository = coursekeyRepository;
            _accountRepository = accountRepository;
            _userManager = userManager;
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
            _payOSApiService = payOSApiService;
            _packageRepository = packageRepository;
        }

        public async Task<BaseResponse<PayOSPaymentReturnResponse>> HandlePaymentAsync(Dictionary<string, string> request, string userId)
        {
            if (request.GetValueOrDefault("code") != "00" || request.GetValueOrDefault("cancel") == "true")
            {
                throw new Exception("Thanh toán thất bại.");
            }
            var paymentId = request.GetValueOrDefault("id");

            var apiRequest = PayOSApiRequest.Builder()
                .CallUrl("/v2/payment-requests/{id}")
                .AddHeader("x-client-id", CommonUtils.GetApiKey("PAYOS_CLIENT_ID"))
                .AddHeader("x-api-key", CommonUtils.GetApiKey("PAYOS_API_KEY"))
                .AddPathVariable("id", paymentId)
                .Build();

            var paymentResponse = await _payOSApiService.GetAsync<PayOSPaymentReturnResponse>(apiRequest);

            var desc = paymentResponse.Data.Transactions.First().Description;

            int? courseId = null;
            int? packageId = null;

            // Parse description để lấy CourseId hoặc PackageId
            var courseMatch = Regex.Match(desc, @"khóa học\s+(\d+)", RegexOptions.IgnoreCase);
            var packageMatch = Regex.Match(desc, @"gói đăng kí\s+(\d+)", RegexOptions.IgnoreCase);

            if (courseMatch.Success)
            {
                courseId = int.Parse(courseMatch.Groups[1].Value);
            }
            else if (packageMatch.Success)
            {
                packageId = int.Parse(packageMatch.Groups[1].Value);
            }
            else
            {
                throw new Exception("Không thể xác định loại thanh toán từ description.");
            }

            var transaction = new Transactionhistory
            {
                UserId = userId,
                Amount = paymentResponse.Data.AmountPaid,
                TransactionDate = DateTime.Parse(paymentResponse.Data.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind)
            };

            // Chỉ lưu transaction nếu có CourseId (vì model yêu cầu CourseId)
            if (courseId.HasValue)
            {
                transaction.ItemId = courseId.Value;

                // Tạo CourseKey khi thanh toán thành công cho khóa học
                var uniqueKeyValue = Guid.NewGuid().ToString("N");
                var courseKey = new Coursekey
                {
                    CourseId = courseId.Value,
                    StudentId = null,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = "available",
                    KeyValue = uniqueKeyValue
                };
                await _coursekeyRepository.AddAsync(courseKey);

                // Gửi key cho Parent
                await SendKeyToParentAsync(userId, uniqueKeyValue, courseId.Value);
            }
            else if (packageId.HasValue)
            {
                transaction.ItemId = packageId.Value;

                var userPackage = new Userpackage
                {
                    UserId = userId,
                    PackageId = packageId.Value,
                    Price = paymentResponse.Data.AmountPaid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _packageRepository.AddUserPackageAsync(userPackage);
            }

            await _paymentRepository.AddAsync(transaction);

            var successMessage = courseId.HasValue 
                ? "Thanh toán thành công! Mã khóa học đã được gửi cho phụ huynh."
                : "Thanh toán thành công! Gói đăng kí đã được kích hoạt.";

            return new BaseResponse<PayOSPaymentReturnResponse>(
                    successMessage,
                    StatusCodeEnum.OK_200,
                    paymentResponse
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

        public async Task<BaseResponse<PayOSApiResponse>> GetPayOSPaymentUrlAsync(PayOSPaymentRequest request, string userId)
        {
            var body = new PayOSApiBody
            {
                OrderCode = CommonUtils.GenerateUniqueOrderCode(),
                Amount = request.Amount,
                BuyerName = userId,
                CancelUrl = "http://localhost:5173/my-courses",
                ReturnUrl = "http://localhost:5173/my-courses"
            };

            if (string.IsNullOrEmpty(request.CourseId.ToString())
                && string.IsNullOrEmpty(request.PackageId.ToString()))
            {
                throw new Exception("Id Khóa học/ gói đăng kí trống.");
            }

            if (!string.IsNullOrEmpty(request.CourseId.ToString()))
            {
                var courseCheck = await _courseRepository.CheckDuplicate(request.CourseId, userId);
                if (courseCheck)
                {
                    throw new Exception("Bạn đã mua khóa học này rồi.");
                }
                
                body.Description = $"Thanh toán cho khóa học {request.CourseId}.";
            }
            else
            {
                var packageCheck = await _packageRepository.CheckDuplicate(request.PackageId, userId);
                if (packageCheck)
                {
                    throw new Exception("Bạn đã mua gói đăng kí này rồi.");
                }

                body.Description = $"Thanh toán cho gói đăng kí {request.PackageId}.";
            }

            var signature = CommonUtils.GeneratePayOSSignature(body, CommonUtils.GetApiKey("PAYOS_CHECKSUMKEY"));

            body.Signature = signature;

            var apiRequest = PayOSApiRequest.Builder()
                .CallUrl("/v2/payment-requests")
                .AddHeader("x-client-id", CommonUtils.GetApiKey("PAYOS_CLIENT_ID"))
                .AddHeader("x-api-key", CommonUtils.GetApiKey("PAYOS_API_KEY"))
                .Body(body)
                .Build();

            var response = await _payOSApiService.PostAsync<PayOSApiBody, PayOSApiResponse>(apiRequest, body);

            return new BaseResponse<PayOSApiResponse>(
                "Thanh toán thành công",
                StatusCodeEnum.OK_200,
                response
                );
        }

        public async Task<BaseResponse<PayOSPayoutApiResponse>> PayOSPayoutAsync(PayoutRequest payoutRequest, string userId)
        {
            var body = new PayOSPayoutApiBody
            {
                ReferenceId = DateTime.Now.Ticks.ToString() + "_" + userId,
                Amount = payoutRequest.Amount,
                Description = $"Payout for {payoutRequest.TeacherID}",
                ToBin = payoutRequest.BankBin,
                ToAccountNumber = payoutRequest.BankAccountNumber,
                Category = new List<string> { "payout" }
            };

            var signature = CommonUtils.CreatePayoutSignature(CommonUtils.GetApiKey("PAYOS_PAYOUT_CHECKSUMKEY"), body.ProcessBody());

            var request = PayOSApiRequest.Builder()
                .CallUrl("/v1/payouts")
                .AddHeader("x-client-id", CommonUtils.GetApiKey("PAYOS_PAYOUT_CLIENT_ID"))
                .AddHeader("x-api-key", CommonUtils.GetApiKey("PAYOS_PAYOUT_API_KEY"))
                .AddHeader("x-idempotency-key", DateTime.Now.Ticks.ToString())
                .AddHeader("x-signature", signature)
                .Body(body)
                .Build();

            var response = await _payOSApiService.PostAsync<PayOSPayoutApiBody, PayOSPayoutApiResponse>(request, body);

            return new BaseResponse<PayOSPayoutApiResponse>(
                "Thanh toán thành công",
                StatusCodeEnum.OK_200,
                response
                );
        }

        public async Task<BaseResponse<PayOSWebhookApiResponse>> CreateOrUpdateWebhookUrl(string url)
        {
            var body = new PayOSWebhookApiBody
            {
                WebhookUrl = url
            };

            var request = PayOSApiRequest.Builder()
                .CallUrl("/confirm-webhook")
                .AddHeader("x-client-id", CommonUtils.GetApiKey("PAYOS_CLIENT_ID"))
                .AddHeader("x-api-key", CommonUtils.GetApiKey("PAYOS_API_KEY"))
                .Body(body)
                .Build();

            var response = await _payOSApiService.PostAsync<PayOSWebhookApiBody, PayOSWebhookApiResponse>(request, body);

            return new BaseResponse<PayOSWebhookApiResponse>(
                "Thanh toán thành công",
                StatusCodeEnum.OK_200,
                response
                );
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
