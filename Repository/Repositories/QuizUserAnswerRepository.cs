using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class QuizUserAnswerRepository : BaseRepository<Quizuseranswer>, IQuizUserAnswerRepository
    {
        private readonly IGCSEContext _context;
        public QuizUserAnswerRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
