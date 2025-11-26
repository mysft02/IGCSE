using BusinessObject;
using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.Model;
using Common.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class FinalQuizResultRepository : BaseRepository<Finalquizresult>, IFinalQuizResultRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public FinalQuizResultRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FinalQuizWithReviewResponse> GetFinalQuizResultWithReview(int finalQuizId, string userId)
        {
            var result = await _context.Finalquizresults
                .Include(x => x.FinalQuiz)
                .Where(x => x.FinalQuizId == finalQuizId && x.UserId == userId && x.IsPassed == true)
                .Select(x => new FinalQuizWithReviewResponse
                {
                    Id = x.FinalQuizId,
                    Title = x.FinalQuiz.Title,
                    Description = x.FinalQuiz.Description,
                    Questions = _context.Finalquizuseranswers
                    .Include(cx => cx.Question)
                    .Where(c => c.FinalQuizResultId == x.FinalQuizResultId)
                    .Select(c => new QuizResultQuestionResponse
                    {
                        QuestionId = c.Question.QuestionId,
                        QuestionContent = c.Question.QuestionContent,
                        ImageUrl = CommonUtils.GetMediaUrl(c.Question.PictureUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
                        CorrectAnswer = c.Question.CorrectAnswer,
                        UserAnswer = new QuizQuestionUserAnswerResponse
                        {
                            QuizUserAnswerId = c.FinalQuizUserAnswerId,
                            UserAnswer = c.Answer
                        }
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            if(result == null)
            {
                return null;
            }

            return result;
        }
    }
}
