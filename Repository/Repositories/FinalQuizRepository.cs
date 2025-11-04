using BusinessObject;
using BusinessObject.Model;
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
    }
}
