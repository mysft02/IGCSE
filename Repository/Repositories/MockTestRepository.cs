using BusinessObject;
using BusinessObject.Model;
using Common.Constants;
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

        public MockTestStatusEnum CheckMockTestDone(int mockTestId, string userId)
        {
            var package = _context.Userpackages.FirstOrDefault(x => x.UserId == userId && x.IsActive == true);

            if(package == null)
            {
                return MockTestStatusEnum.Locked;
            }

            var result = _context.Mocktestresults.Any(x => x.MockTestId == mockTestId && x.UserId == userId);

            if(result)
            {
                return MockTestStatusEnum.Completed;
            }
            return MockTestStatusEnum.Open;
        }
    }
}
