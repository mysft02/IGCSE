using BusinessObject.Model;
using Repository.IBaseRepository;
using BusinessObject.DTOs.Response.Quizzes;

namespace Repository.IRepositories
{
    public interface IQuizResultRepository : IBaseRepository<Quizresult>
    {
        Task<QuizResultReviewResponse?> GetQuizResultWithReviewAsync(int quizId, string userId);
    }
}
