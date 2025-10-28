using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class MockTestRepository : BaseRepository<Mocktest>, IMockTestRepository
    {
        private readonly IGCSEContext _context;

        public MockTestRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
