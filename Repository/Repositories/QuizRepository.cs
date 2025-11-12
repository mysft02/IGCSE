using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.DTOs.Response.FinalQuizzes;

namespace Repository.Repositories
{
    public class QuizRepository : BaseRepository<Quiz>, IQuizRepository
    {
        private readonly IGCSEContext _context;

        public QuizRepository(IGCSEContext context) : base(context)
        {
            _context = context;
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
                        ImageUrl = c.PictureUrl
                    })
                    .ToList(),
                })
                .FirstOrDefaultAsync();

            return quiz;
        }

        public async Task<bool> CheckAllowance(string userId, int quizId)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(x => x.QuizId == (int)quizId);

            var result = await _context.Processes
                .AnyAsync(x => x.LessonId  == quiz.LessonId && x.StudentId == userId && x.IsUnlocked == true);

            return result;
        }
    }
}
