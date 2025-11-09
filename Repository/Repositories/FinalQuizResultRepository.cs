using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class FinalQuizResultRepository : BaseRepository<Finalquizresult>, IFinalQuizResultRepository
    {
        private readonly IGCSEContext _context;

        public FinalQuizResultRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
