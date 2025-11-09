using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.IBaseRepository;
using System.Linq.Expressions;

namespace Repository.IRepositories
{
    public interface IMockTestRepository : IBaseRepository<Mocktest>
    {
        Task<Mocktest?> GetByMockTestIdAsync(int mockTestId);

        bool CheckMockTestDone(int mockTestId, string userId);
    }
}
