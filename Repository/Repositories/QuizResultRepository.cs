using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class QuizResultRepository : BaseRepository<Quizresult>, IQuizResultRepository
    {
        private readonly IGCSEContext _context;

        public QuizResultRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
