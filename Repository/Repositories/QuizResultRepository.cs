using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;
using BusinessObject.DTOs.Response.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Common.Utils;
using Microsoft.AspNetCore.Http;

namespace Repository.Repositories
{
    public class QuizResultRepository : BaseRepository<Quizresult>, IQuizResultRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizResultRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<QuizResultReviewResponse?> GetQuizResultWithReviewAsync(int quizId, string userId)
        {
            // Lấy quiz result mới nhất của user cho quiz này
            var quizResult = await _context.Quizresults
                .Where(x => x.QuizId == quizId && x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (quizResult == null)
            {
                return null;
            }

            // Lấy quiz với questions
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.QuizId == quizId)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                return null;
            }

            // Lấy tất cả user answers cho quiz này (lấy mới nhất cho mỗi question)
            var userAnswers = await _context.Quizuseranswers
                .Where(ua => ua.QuizId == quizId)
                .GroupBy(ua => ua.QuestionId)
                .Select(g => g.OrderByDescending(ua => ua.QuizUserAnswerId).First())
                .ToListAsync();

            var result = new QuizResultReviewResponse
            {
                QuizResultId = quizResult.QuizResultId,
                Score = quizResult.Score,
                IsPassed = quizResult.IsPassed,
                DateTaken = quizResult.CreatedAt,
                Quiz = new QuizResultReviewDetailResponse
                {
                    QuizId = quiz.QuizId,
                    Title = quiz.QuizTitle,
                    Description = quiz.QuizDescription,
                    Questions = quiz.Questions
                    .Select(q => new QuizResultQuestionResponse
                    {
                        QuestionId = q.QuestionId,
                        QuestionContent = q.QuestionContent,
                        ImageUrl = CommonUtils.GetMediaUrl(q.PictureUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
                        CorrectAnswer = quizResult.IsPassed ? q.CorrectAnswer : null, // Chỉ trả về đáp án đúng nếu pass
                        UserAnswer = userAnswers
                            .Where(ua => ua.QuestionId == q.QuestionId)
                            .Select(ua => new QuizQuestionUserAnswerResponse
                            {
                                QuizUserAnswerId = ua.QuizUserAnswerId,
                                UserAnswer = ua.Answer
                            }).FirstOrDefault()
                    }).ToList()
                }
            };

            return result;
        }
    }
}
