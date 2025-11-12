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
    }
}
