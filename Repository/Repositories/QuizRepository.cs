using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public class QuizRepository : BaseRepository<Quiz>, IQuizRepository
    {
        private readonly IGCSEContext _context;

        public QuizRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetByQuizIdAsync(int quizId)
        {
            return _context.Quizzes
                .Include(x => x.Questions)
                .FirstOrDefault(x => x.QuizId == quizId);
        }
    }
}
