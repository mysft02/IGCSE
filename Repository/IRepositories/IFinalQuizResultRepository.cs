using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IFinalQuizResultRepository : IBaseRepository<Finalquizresult>
    {
        Task<FinalQuizWithReviewResponse> GetFinalQuizResultWithReview(int finalQuizId, string userId);
    }
}
