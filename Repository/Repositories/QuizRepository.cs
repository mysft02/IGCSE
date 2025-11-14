using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.DTOs.Response.FinalQuizzes;
using Microsoft.AspNetCore.Hosting;
using Common.Utils;
using Microsoft.AspNetCore.Http;

namespace Repository.Repositories
{
    public class QuizRepository : BaseRepository<Quiz>, IQuizRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<QuizResponse> GetByQuizIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Where(x => x.QuizId == quizId)
                .Include(c => c.Questions)
                .Select(x => new QuizResponse()
                {
                    Id = x.QuizId,
                    Title = x.QuizTitle,
                    Description = x.QuizDescription,
                    Questions = x.Questions
                    .Select(c => new FinalQuizQuestionResponse
                    {
                        QuestionId = c.QuestionId,
                        QuestionContent = c.QuestionContent,
                        ImageUrl = CommonUtils.GetMediaUrl(c.PictureUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor)
                    })
                    .ToList(),
                })
                .FirstOrDefaultAsync();

            return quiz;
        }

        public async Task<bool> CheckAllowance(string userId, int quizId)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(x => x.QuizId == (int)quizId);

            var lessonItemCount = await _context.Lessonitems.Where(x => x.LessonId == quiz.LessonId).CountAsync();

            var processItemCount = await _context.Processitems.Where(x => x.LessonItem.LessonId == quiz.LessonId && x.Process.StudentId == userId).CountAsync();

            if(lessonItemCount == processItemCount)
            {
                return true;
            }

            return false;
        }
    }
}
