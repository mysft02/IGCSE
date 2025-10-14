using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
    {
        private readonly IGCSEContext _context;

        public QuestionRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
