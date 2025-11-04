using BusinessObject.Model;
using BusinessObject;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class MockTestResultRepository : BaseRepository<Mocktestresult>, IMockTestResultRepository
    {
        private readonly IGCSEContext _context;

        public MockTestResultRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
