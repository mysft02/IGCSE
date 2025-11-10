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
using BusinessObject.DTOs.Response.Payment;
using BusinessObject.DTOs.Response.Courses;

namespace Service
{
    public class PaymentService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly PayOSApiService _payOSApiService;
        private readonly IParentStudentLinkRepository _parentStudentLinkRepository;
        private readonly CourseRegistrationService _courseRegistrationService;

        public PaymentService(
            IAccountRepository accountRepository,
            UserManager<Account> userManager,
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            PayOSApiService payOSApiService,
            IParentStudentLinkRepository parentStudentLinkRepository,
            CourseRegistrationService courseRegistrationService)
        {
            
            _accountRepository = accountRepository;
            _userManager = userManager;
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
            _payOSApiService = payOSApiService;
            _parentStudentLinkRepository = parentStudentLinkRepository;
            _courseRegistrationService = courseRegistrationService;
        }

        public async Task<BaseResponse<PayOSPaymentReturnResponse>> HandlePaymentAsync(Dictionary<string, string> request, string userId)
        {
            if (request.GetValueOrDefault("code") != "00" || request.GetValueOrDefault("cancel") != "true")
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

            var m = Regex.Match(desc, @"Course\s*id\s*:\s*(\S+)", RegexOptions.IgnoreCase);
            var courseId = int.Parse(m.Success ? m.Groups[1].Value : null);

            var transaction = new Transactionhistory
            {
                CourseId = courseId,
                UserId = userId,
                Amount = paymentResponse.Data.AmountPaid,
                TransactionDate = DateTime.Parse(paymentResponse.Data.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind)
            };

            await _paymentRepository.AddAsync(transaction);

            // Chỉ Parent mới được mua khóa học cho con
            // Tự động gán khóa học cho các học sinh liên kết với Parent
            var linkedStudents = await _parentStudentLinkRepository.GetByParentId(userId);
            
            if (linkedStudents == null || !linkedStudents.Any())
            {
                return new BaseResponse<PayOSPaymentReturnResponse>(
                    "Thanh toán thành công nhưng không tìm thấy học sinh liên kết. Vui lòng liên kết học sinh trước khi mua khóa học.",
                    StatusCodeEnum.BadRequest_400,
                    paymentResponse
                );
            }

            foreach (var student in linkedStudents)
            {
                await _courseRegistrationService.InitializeCourseProgressForStudentAsync(student.Id, courseId);
            }

            return new BaseResponse<PayOSPaymentReturnResponse>(
                $"Thanh toán thành công! Khóa học đã được gán cho {linkedStudents.Count()} học sinh.",
                StatusCodeEnum.OK_200,
                paymentResponse
            );
        }

        // Course key-related methods removed

        public async Task<BaseResponse<PaymentAnalyticsResponse>> GetPaymentAnalyticsAsync()
        {
            var analytics = await _paymentRepository.GetPaymentSortedByDate();

            var response = new PaymentAnalyticsResponse
            {
                PaymentAnalytics = analytics
            };

            return new BaseResponse<PaymentAnalyticsResponse>(
                    "Thành công",
                    StatusCodeEnum.OK_200,
                    response
                );
        }

        public async Task<BaseResponse<IEnumerable<TransactionHistoryResponse>>> GetAllTransactionHistoriesByUserId(string userId)
        {
            var transactionHistories = await _paymentRepository.GetAllTransactionHistoriesByUserId(userId);

            return new BaseResponse<IEnumerable<TransactionHistoryResponse>>(
                "Thành công",
                StatusCodeEnum.OK_200,
                transactionHistories
            );
        }

        public async Task<BaseResponse<PayOSApiResponse>> GetPayOSPaymentUrlAsync(PayOSPaymentRequest request, string userId)
        {
            var body = new PayOSApiBody
            {
                OrderCode = CommonUtils.GenerateUniqueOrderCode(),
                Amount = request.Amount,
                BuyerName = userId,
                Description = $"Course id: {request.CourseId} payment",
                CancelUrl = "http://localhost:5173/my-courses",
                ReturnUrl = "http://localhost:5173/my-courses"
            };

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
