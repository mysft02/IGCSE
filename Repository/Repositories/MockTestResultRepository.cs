using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;
using BusinessObject.DTOs.Response.MockTest;
using Microsoft.EntityFrameworkCore;
using BusinessObject.DTOs.Response.MockTestQuestion;
using Common.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Repository.Repositories
{
    public class MockTestResultRepository : BaseRepository<Mocktestresult>, IMockTestResultRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MockTestResultRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MockTestResultReviewResponse> GetMockTestResultWithReview(int id)
        {
            var result = await _context.Mocktestresults
                .Include(x => x.MockTest)
                    .ThenInclude(mt => mt.MockTestQuestions)
                .Include(x => x.MockTestUserAnswer)
                .Select(x => new MockTestResultReviewResponse
                {
                    MockTestResultId = x.MockTestResultId,
                    MockTest = new MockTestResultReviewDetailResponse
                    {
                        MockTestId = x.MockTest.MockTestId,
                        MockTestTitle = x.MockTest.MockTestTitle,
                        MockTestDescription = x.MockTest.MockTestDescription,
                        CreatedAt = x.MockTest.CreatedAt,
                        UpdatedAt = x.MockTest.UpdatedAt,
                        CreatedBy = x.MockTest.CreatedBy,
                        Questions = x.MockTest.MockTestQuestions
                        .Select(c => new MockTestResultQuestionResponse
                        {
                            QuestionId = c.MockTestQuestionId,
                            QuestionContent = c.QuestionContent,
                            CorrectAnswer = c.CorrectAnswer,
                            ImageUrl = string.IsNullOrEmpty(c.ImageUrl) ? "" : CommonUtils.GetMediaUrl(c.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
                            Mark = c.Mark,
                            PartialMark = c.PartialMark,
                            UserAnswer = x.MockTestUserAnswer
                            .Where(xc => xc.MockTestQuestionId == c.MockTestQuestionId)
                            .Select(q => new MockTestQuestionUserAnswerResponse
                            {
                                MockTestUserAnswerId = q.MockTestUserAnswerId,
                                UserAnswer = q.Answer,
                                UserMark = q.Score
                            }).FirstOrDefault()
                        }).ToList()
                    },
                    Score = x.Score,
                    DateTaken = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<int?> GetLatestMockTestResultIdAsync(int mockTestId, string userId)
        {
            var latestResult = await _context.Mocktestresults
                .Where(x => x.MockTestId == mockTestId && x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.MockTestResultId)
                .FirstOrDefaultAsync();

            return latestResult == 0 ? null : latestResult;
        }
    }
}
