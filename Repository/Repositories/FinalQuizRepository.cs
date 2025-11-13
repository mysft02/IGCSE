using BusinessObject;
using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class FinalQuizRepository : BaseRepository<Finalquiz>, IFinalQuizRepository
    {
        private readonly IGCSEContext _context;

        public FinalQuizRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FinalQuizResponse> GetFinalQuizAsync(int id)
        {
            var finalQuiz = await _context.Finalquizzes
                .Where(x => x.FinalQuizId == id)
                .Select(x => new FinalQuizResponse()
                {
                    Id = x.FinalQuizId,
                    Title = x.Title,
                    Description = x.Description,
                    Questions = _context.Questions
                    .Where(xc => xc.Quiz.CourseId == x.CourseId)
                    .Select(c => new FinalQuizQuestionResponse
                    {
                        QuestionId = c.QuestionId,
                        QuestionContent = c.QuestionContent,
                        ImageUrl = c.PictureUrl
                    })
                    .Take(20)
                    .ToList(),
                })
                .FirstOrDefaultAsync();

            return finalQuiz;
        }

        public async Task<bool> CheckAllowance(int finalQuizId, string userId)
        {
            var finalLesson = await _context.Finalquizzes
                .Include(x => x.Course)
                .Include(x => x.Course.CourseSections).ThenInclude(xc => xc.Lessons)
                .SelectMany(x => x.Course.CourseSections)
                .SelectMany(x => x.Lessons)
                .OrderByDescending(c => c.Order)
                .FirstOrDefaultAsync();

            var lessonItemCount = await _context.Lessonitems.Where(x => x.LessonId == finalLesson.LessonId).CountAsync();

            var processItemCount = await _context.Processitems.Where(x => x.LessonItem.LessonId == finalLesson.LessonId && x.Process.StudentId == userId).CountAsync();

            if (lessonItemCount == processItemCount)
            {
                return true;
            }

            return false;
        }
    }
}
