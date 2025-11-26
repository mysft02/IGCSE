using BusinessObject;
using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.Model;
using Common.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Text.RegularExpressions;

namespace Repository.Repositories
{
    public class FinalQuizRepository : BaseRepository<Finalquiz>, IFinalQuizRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FinalQuizRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
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
                    .Include(c => c.Quiz)
                    .Where(xc => xc.Quiz.CourseId == x.CourseId)
                    .Select(c => new FinalQuizQuestionResponse
                    {
                        QuestionId = c.QuestionId,
                        QuestionContent = c.QuestionContent,
                        ImageUrl = CommonUtils.GetMediaUrl(c.PictureUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor)
                    })
                    .OrderBy(q => EF.Functions.Random())
                    .Take(10)
                    .ToList(),
                })
                .FirstOrDefaultAsync();

            var questions = finalQuiz.Questions;
            for (int i = 0; i < questions.Count; i++)
            {
                string newIndex = (i + 1).ToString();

                questions[i].QuestionContent = Regex.Replace(
                    questions[i].QuestionContent,
                    @"Câu\s*\d+\s*:",
                    $"Câu {newIndex}:"
                );
            }

            return finalQuiz;
        }

        public async Task<bool> CheckAllowance(int finalQuizId, string userId)
        {
            var finalQuiz = await _context.Finalquizzes
                .Include(x => x.Course).ThenInclude(xc => xc.CourseSections).ThenInclude(p => p.Lessons).ThenInclude(pc => pc.Lessonitems)
                .FirstOrDefaultAsync(x => x.FinalQuizId == finalQuizId);
            if(finalQuiz == null)
            {
                return false;
            }

            var finalLesson = finalQuiz.Course.CourseSections
                .OrderByDescending(x => x.Order)
                .FirstOrDefault()
                .Lessons
                .OrderByDescending(x => x.Order)
                .FirstOrDefault();

            var finalLessonItem = finalLesson.Lessonitems
                .OrderByDescending(x => x.Order)
                .FirstOrDefault();

            var process = await _context.Processes
                .Include(x => x.Processitems)
                .FirstOrDefaultAsync(x => x.LessonId == finalLesson.LessonId && x.StudentId == userId);
            if(process != null && process.IsUnlocked == false)
            {
                return false;
            }

            var processItem = process.Processitems
                .Where(x => x.LessonItemId == finalLessonItem.LessonItemId)
                .FirstOrDefault();
            if (processItem == null)
            {
                return false;
            }

            return true;
        }
    }
}
