using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class FinalQuizUserAnswerRepository : BaseRepository<Finalquizuseranswer>, IFinalQuizUserAnswerRepository
    {
        private readonly IGCSEContext _context;

        public FinalQuizUserAnswerRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
