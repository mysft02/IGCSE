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
using BusinessObject.DTOs.Request.Payments;

namespace Service
{
    public class PaymentService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly PayOSApiService _payOSApiService;
        private readonly IPackageRepository _packageRepository;
        private readonly IParentStudentLinkRepository _parentStudentLinkRepository;
        private readonly CourseService _courseService;

        public PaymentService(
            IAccountRepository accountRepository,
            UserManager<Account> userManager,
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            PayOSApiService payOSApiService,
            IPackageRepository packageRepository,
            IParentStudentLinkRepository parentStudentLinkRepository,
            CourseService courseService)
        {
            _accountRepository = accountRepository;
            _userManager = userManager;
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
            _payOSApiService = payOSApiService;
            _packageRepository = packageRepository;
            _parentStudentLinkRepository = parentStudentLinkRepository;
            _courseService = courseService;
        }

        public async Task<BaseResponse<PayOSPaymentReturnResponse>> HandlePaymentAsync(PaymentCallBackRequest request, string userId)
        {
            if(request == null)
            {
                throw new Exception("Thông tin thanh toán không có.");
            }

            if (request.Code != "00" || request.Cancel == "true")
            {
                throw new Exception("Thanh toán thất bại.");
            }
            var paymentId = request.Id;

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
            var courseMatch = Regex.Match(desc, @"khoa hoc\s*(\d+)", RegexOptions.IgnoreCase);
            var packageMatch = Regex.Match(desc, @"go[iíìỉĩ]\s*(\d+)", RegexOptions.IgnoreCase);


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

                // Tự động enroll tất cả students liên kết với Parent vào khóa học
                var linkedStudents = await _parentStudentLinkRepository.GetByParentId(userId);
                
                foreach (var student in linkedStudents)
                {
                    try
                    {
                        // Khởi tạo tiến trình học cho từng student
                        await _courseService.InitializeCourseProgressForStudentAsync(student.Id, courseId.Value);
                    }
                    catch (Exception ex)
                    {
                        // Log error nhưng không throw để không ảnh hưởng đến các student khác
                        Console.WriteLine($"Error enrolling student {student.Id} to course {courseId.Value}: {ex.Message}");
                    }
                }
            }
            else if (packageId.HasValue)
            {
                transaction.ItemId = packageId.Value;

                var userPackage = new Userpackage
                {
                    UserId = userId,
                    PackageId = packageId.Value,
                    Price = paymentResponse.Data.AmountPaid,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _packageRepository.AddUserPackageAsync(userPackage);
            }

            await _paymentRepository.AddAsync(transaction);

            var successMessage = courseId.HasValue 
                ? "Thanh toán thành công! Các học sinh đã được đăng ký vào khóa học."
                : "Thanh toán thành công! Gói đăng kí đã được kích hoạt.";

            return new BaseResponse<PayOSPaymentReturnResponse>(
                    successMessage,
                    StatusCodeEnum.OK_200,
                    paymentResponse
                );
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

            if (!string.IsNullOrEmpty(request.CourseId?.ToString()))
            {
                // Kiểm tra xem parent đã mua khóa học này chưa bằng cách kiểm tra transaction history
                var existingTransaction = await _paymentRepository.GetByUserAndCourseAsync(userId, request.CourseId.Value);
                if (existingTransaction != null)
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

                body.Description = $"Thanh toán cho gói {request.PackageId}.";
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
    }
}
