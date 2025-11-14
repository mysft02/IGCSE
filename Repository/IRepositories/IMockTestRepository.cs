using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.Model;
using Common.Constants;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IMockTestRepository : IBaseRepository<Mocktest>
    {
        Task<MockTestForStudentResponse?> GetByMockTestIdAsync(int mockTestId);

        MockTestStatusEnum CheckMockTestDone(int mockTestId, string userId);
    }
}
