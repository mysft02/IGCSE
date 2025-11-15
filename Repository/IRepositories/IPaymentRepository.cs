using BusinessObject.DTOs.Response.Payment;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IPaymentRepository : IBaseRepository<Transactionhistory>
    {
        Task<Transactionhistory?> GetByUserAndCourseAsync(string userId, int courseId);
    }
}
