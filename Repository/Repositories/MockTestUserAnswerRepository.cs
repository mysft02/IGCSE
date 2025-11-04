using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class MockTestUserAnswerRepository : BaseRepository<Mocktestuseranswer>, IMockTestUserAnswerRepository
    {
        private readonly IGCSEContext _context;

        public MockTestUserAnswerRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
