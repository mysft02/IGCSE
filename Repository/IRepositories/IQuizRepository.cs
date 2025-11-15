using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IQuizRepository : IBaseRepository<Quiz>
    {
        Task<QuizResponse?> GetByQuizIdAsync(int quizId);
        Task<bool> CheckAllowance(string userId, int quizId);
    }
}
