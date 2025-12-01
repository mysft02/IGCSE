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
using BusinessObject.DTOs.Request.Payments;
using BusinessObject.DTOs.Response.Payment;
using Org.BouncyCastle.Asn1.Ocsp;
using BusinessObject.DTOs.Response.MockTest;
using Repository.Repositories;

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
        private readonly IStudentEnrollmentRepository _studentEnrollmentRepository;
        private readonly IProcessRepository _processRepository;
        private readonly IUserPackageRepository _userPackageRepository;
        private readonly ICreateSlotRepository _createSlotRepository;
        private readonly IPaymentInformationRepository _paymentInformationRepository;
        private readonly IPayoutHistoryRepository _payoutHistoryRepository;

        public PaymentService(
            IAccountRepository accountRepository,
            UserManager<Account> userManager,
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            PayOSApiService payOSApiService,
            IPackageRepository packageRepository,
            IParentStudentLinkRepository parentStudentLinkRepository,
            CourseService courseService,
            IStudentEnrollmentRepository studentEnrollmentRepository,
            IProcessRepository processRepository,
            IUserPackageRepository userPackageRepository,
            ICreateSlotRepository createSlotRepository,
            IPaymentInformationRepository paymentInformationRepository,
            IPayoutHistoryRepository payoutHistoryRepository)
        {
            _accountRepository = accountRepository;
            _userManager = userManager;
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
            _payOSApiService = payOSApiService;
            _packageRepository = packageRepository;
            _parentStudentLinkRepository = parentStudentLinkRepository;
            _courseService = courseService;
            _studentEnrollmentRepository = studentEnrollmentRepository;
            _processRepository = processRepository;
            _userPackageRepository = userPackageRepository;
            _createSlotRepository = createSlotRepository;
            _paymentInformationRepository = paymentInformationRepository;
            _payoutHistoryRepository = payoutHistoryRepository;
        }

        public async Task<BaseResponse<PayOSPaymentReturnResponse>> HandlePaymentAsync(PaymentCallBackRequest request, string userId, string userRole)
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
            var orderCode = paymentResponse.Data.OrderCode.ToString();

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

            if (courseId.HasValue)
            {
                transaction.ItemId = courseId.Value;
                var course = await _courseRepository.FindOneAsync(x => x.CourseId == courseId);
                if(course == null)
                {
                    throw new Exception("Không tìm thấy khoá học này");
                }
                var teacherPaymentInfo = await _paymentInformationRepository.FindOneAsync(x => x.UserId == course.CreatedBy);

                var studentEnroll = await _studentEnrollmentRepository.FindOneAsync(x => x.ParentId.Contains("_") && x.ParentId.Substring(x.ParentId.IndexOf("_") + 1) == orderCode);
                studentEnroll.ParentId = studentEnroll.ParentId.Split('_')[0];

                await _studentEnrollmentRepository.UpdateAsync(studentEnroll);
                await _courseService.InitializeCourseProgressForStudentAsync(studentEnroll.StudentId, courseId.Value);
                var teacherIncome = (paymentResponse.Data.AmountPaid * 70) / 100;
                var payoutData = new PayoutRequest
                {
                    Amount = teacherIncome,
                    TeacherID = course.CreatedBy,
                    BankBin = teacherPaymentInfo.BankBin,
                    BankAccountNumber = teacherPaymentInfo.BankAccountNumber,
                };

                var payout = await PayOSPayoutAsync(payoutData);
                var payoutHistory = new Payouthistory
                {
                    TeacherId = course.CreatedBy,
                    CourseId = course.CourseId,
                    Amount = teacherIncome,
                    CreatedAt = DateTime.Now,
                };
                await _payoutHistoryRepository.AddAsync(payoutHistory);
            }
            else if (packageId.HasValue)
            {
                transaction.ItemId = packageId.Value;

                var package = await _packageRepository.FindOneWithIncludeAsync(x => x.PackageId == packageId, xc => xc.Userpackages);
                if(package == null)
                {
                    throw new Exception("Gói bạn cần mua không tìm thấy.");
                }

                var userPackage = package.Userpackages
                        .FirstOrDefault(x => x.UserId.Contains("_") && x.UserId.Substring(x.UserId.IndexOf("_") + 1) == orderCode);

                var newUserPackage = new Userpackage
                {
                    PackageId = packageId.Value,
                    UserId = userId,
                    Price = paymentResponse.Data.AmountPaid,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                if (userRole == "Parent")
                {
                    newUserPackage.UserId = userPackage.UserId.Split("_")[0];

                    await _userPackageRepository.AddAsync(newUserPackage);
                    await _userPackageRepository.DeleteAsync(userPackage);
                }
                else
                {
                    var teacherPackage = package.Userpackages.FirstOrDefault(x => x.UserId == userId);

                    if(teacherPackage != null)
                    {
                        teacherPackage.Price = paymentResponse.Data.AmountPaid;
                        teacherPackage.IsActive = true;
                        teacherPackage.UpdatedAt = DateTime.UtcNow;

                        await _userPackageRepository.UpdateAsync(teacherPackage);
                    }
                    else
                    {
                        await _userPackageRepository.AddAsync(newUserPackage);
                        await _userPackageRepository.DeleteAsync(userPackage);
                    }

                    var currSlot = await _createSlotRepository.FindOneAsync(x => x.TeacherId == userId);
                    if (currSlot != null)
                    {
                        currSlot.Slot += package.Slot;
                        currSlot.AvailableSlot += package.Slot;

                        await _createSlotRepository.UpdateAsync(currSlot);
                    }
                    else
                    {
                        var newSlot = new Createslot
                        {
                            Slot = package.Slot,
                            AvailableSlot = package.Slot,
                            TeacherId = userId,
                            PackageId = packageId.Value
                        };

                        await _createSlotRepository.AddAsync(newSlot);
                    }
                }
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

        public async Task<BaseResponse<PayOSApiResponse>> GetPayOSPaymentUrlAsync(PayOSPaymentRequest request, string userId, string userRole)
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
                if (request.DestUserId == null)
                {
                    throw new Exception("Tài khoản học sinh không thể bỏ trống.");
                }

                if (userRole == "Teacher")
                {
                    throw new Exception("Bạn là giáo viên không thể mua khoá học. Vui lòng thử lại sau.");
                }

                if (await _courseRepository.FindOneAsync(x => x.CourseId == request.CourseId) == null)
                {
                    throw new Exception("Không tìm thấy khoá học này.");
                }

                var existed = await _studentEnrollmentRepository.FindOneAsync(x => x.StudentId == request.DestUserId && x.CourseId == request.CourseId);
                if(existed == null)
                {
                    var enroll = new Studentenrollment
                    {
                        StudentId = request.DestUserId,
                        CourseId = (int)request.CourseId,
                        EnrolledAt = DateTime.Now,
                        ParentId = userId + "_" + body.OrderCode
                    };

                    await _studentEnrollmentRepository.AddAsync(enroll);
                }
                else
                {
                    existed.ParentId = userId + "_" + body.OrderCode;
                    await _studentEnrollmentRepository.UpdateAsync(existed);
                }

                body.Description = $"Thanh toán khoá học {request.CourseId}.";
            }
            else
            {
                var package = await _packageRepository.FindOneWithIncludeAsync(x => x.PackageId == request.PackageId, xc => xc.Userpackages);

                if (package == null)
                {
                    throw new Exception("Gói bạn cần mua không tìm thấy");
                }

                if (userRole == "Parent")
                {
                    if (request.DestUserId == null)
                    {
                        throw new Exception("Tài khoản học sinh không thể bỏ trống.");
                    }

                    if (package.IsMockTest == false)
                    {
                        throw new Exception("Gói này dành cho giáo viên. Bạn không thể mua.");
                    }

                    var packageCheck = package.Userpackages.FirstOrDefault(x => x.UserId == request.DestUserId);

                    if (packageCheck != null)
                    {
                        throw new Exception("Bạn đã mua gói này rồi.");
                    }

                    var userPackage = new Userpackage
                    {
                        PackageId = request.PackageId,
                        UserId = request.DestUserId + "_" + body.OrderCode,
                        IsActive = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    await _userPackageRepository.AddOrUpdateAsync(userPackage, c => new object[] { c.PackageId, c.UserId });
                }
                else
                {
                    if (package.IsMockTest == true)
                    {
                        throw new Exception("Gói này dành cho phụ huynh. Bạn không thể mua.");
                    }
                }

                body.Description = $"Thanh toán gói {request.PackageId}.";
            }
            // Kiểm tra API keys trước khi tạo signature
            var checksumKey = CommonUtils.GetApiKey("PAYOS_CHECKSUMKEY");
            var clientId = CommonUtils.GetApiKey("PAYOS_CLIENT_ID");
            var apiKey = CommonUtils.GetApiKey("PAYOS_API_KEY");

            var signature = CommonUtils.GeneratePayOSSignature(body, checksumKey);

            body.Signature = signature;

            var apiRequest = PayOSApiRequest.Builder()
                .CallUrl("/v2/payment-requests")
                .AddHeader("x-client-id", clientId)
                .AddHeader("x-api-key", apiKey)
                .Body(body)
                .Build();

            var response = await _payOSApiService.PostAsync<PayOSApiBody, PayOSApiResponse>(apiRequest, body);

            return new BaseResponse<PayOSApiResponse>(
                "Thanh toán thành công",
                StatusCodeEnum.OK_200,
                response
                );
        }

        public async Task<PayOSPayoutApiResponse> PayOSPayoutAsync(PayoutRequest payoutRequest)
        {
            var body = new PayOSPayoutApiBody
            {
                ReferenceId = DateTime.Now.Ticks.ToString(),
                Amount = payoutRequest.Amount,
                Description = $"Course payment",
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
            return response;
        }

        public async Task<BaseResponse<PaginatedResponse<TransactionHistoryResponse>>> GetTransactionHistory(TransactionHistoryQueryRequest request)
        {
            var filter = request.BuildFilter<Transactionhistory>();

            // Get total count first (for pagination info)
            var totalCount = await _paymentRepository.CountAsync(filter);

            // Get filtered data with pagination
            var items = await _paymentRepository.FindWithPagingAsync(
            filter,
                request.Page,
                request.GetPageSize()
            );

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items);

            var itemList = sortedItems
                .Select(token => new TransactionHistoryResponse
                {
                    TransactionId = token.TransactionId,
                    Amount = token.Amount,
                    TransactionDate = token.TransactionDate,
                })
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<TransactionHistoryResponse>>
            {
                Data = new PaginatedResponse<TransactionHistoryResponse>
                {
                    Items = itemList,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Lấy lịch sử thanh toán thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }
    }
}
