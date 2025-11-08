using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Mocktest?> GetByMockTestIdAsync(int mockTestId)
        {
            return _context.Mocktests
                .Include(x => x.MockTestQuestions)
                .FirstOrDefault(x => x.MockTestId == mockTestId);
        }

        public bool CheckMockTestDone(int mockTestId, string userId)
        {
            return _context.Mocktestresults.Any(x => x.MockTestId == mockTestId && x.UserId == userId);
        }
    }
}
