using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class MockTestQuestionRepository : BaseRepository<Mocktestquestion>, IMockTestQuestionRepository
    {
        private readonly IGCSEContext _context;

        public MockTestQuestionRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}