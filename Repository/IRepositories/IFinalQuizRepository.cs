using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IFinalQuizRepository : IBaseRepository<Finalquiz>
    {
        Task<FinalQuizResponse> GetFinalQuizAsync(int id);
    }
}
