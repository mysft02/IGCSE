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
    }
}
