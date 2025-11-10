using BusinessObject.Model;
using Common.Constants;
using Microsoft.EntityFrameworkCore;
using Repository.IBaseRepository;
using System.Linq.Expressions;

namespace Repository.IRepositories
{
    public interface IMockTestRepository : IBaseRepository<Mocktest>
    {
        Task<Mocktest?> GetByMockTestIdAsync(int mockTestId);

        MockTestStatusEnum CheckMockTestDone(int mockTestId, string userId);
    }
}
