using BusinessObject;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.DTOs.Response.MockTestQuestion;
using BusinessObject.Model;
using Common.Constants;
using Common.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class MockTestRepository : BaseRepository<Mocktest>, IMockTestRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MockTestRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MockTestForStudentResponse?> GetByMockTestIdAsync(int mockTestId)
        {
            return await _context.Mocktests
                .Include(x => x.MockTestQuestions)
                .Where(x => x.MockTestId == mockTestId)
                .Select(x => new MockTestForStudentResponse
                {
                    MockTestId = x.MockTestId,
                    MockTestTitle = x.MockTestTitle,
                    MockTestDescription = x.MockTestDescription,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    CreatedBy = x.CreatedBy,
                    MockTestQuestions = x.MockTestQuestions
                    .Select(xc => new MockTestQuestionResponse
                    {
                        MockTestQuestionId = xc.MockTestQuestionId,
                        QuestionContent = xc.QuestionContent,
                        ImageUrl = string.IsNullOrEmpty(xc.ImageUrl) ? "" : CommonUtils.GetMediaUrl(xc.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),

                        CreatedAt = xc.CreatedAt,
                        UpdatedAt = xc.UpdatedAt,
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public MockTestStatusEnum CheckMockTestDone(int mockTestId, string userId)
        {
            var package = _context.Userpackages.FirstOrDefault(x => x.UserId == userId && x.IsActive == true);
            if(package == null)
            {
                return MockTestStatusEnum.Locked;
            }

            var result = _context.Mocktestresults.Any(x => x.MockTestId == mockTestId && x.UserId == userId);

            if(result)
            {
                return MockTestStatusEnum.Completed;
            }
            return MockTestStatusEnum.Open;
        }

        public MockTestStatusEnum CheckMockTestStatus(int mockTestId, string userId, string userRole)
        {
            if(userRole == "Admin" || userRole == "Teacher" || userRole == "Manager")
            {
                return MockTestStatusEnum.Open;
            }
            var package = _context.Userpackages.FirstOrDefault(x => x.UserId == userId && x.IsActive == true);
            if (package == null)
            {
                return MockTestStatusEnum.Locked;
            }

            var result = _context.Mocktestresults.Any(x => x.MockTestId == mockTestId && x.UserId == userId);

            if (result)
            {
                return MockTestStatusEnum.Completed;
            }
            return MockTestStatusEnum.Open;
        }

        public async Task<MockTestWithAnswerResponse> GetMockTestResultWithReviewAsync(int mockTestId)
        {
            var result = await _context.Mocktests
                .Include(x => x.MockTestQuestions)
                .Select(x => new MockTestWithAnswerResponse
                {
                    MockTestId = x.MockTestId,
                    MockTestDescription = x.MockTestDescription,
                    MockTestTitle = x.MockTestTitle,
                    CreatedAt = x.CreatedAt,
                    Questions = x.MockTestQuestions
                    .Select(c => new MockTestQuestionWithAnswerResponse
                    {
                        QuestionId = c.MockTestQuestionId,
                        QuestionContent = c.QuestionContent,
                        ImageUrl = string.IsNullOrEmpty(c.ImageUrl) ? "" : CommonUtils.GetMediaUrl(c.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
                        CorrectAnswer = c.CorrectAnswer
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
