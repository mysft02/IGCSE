using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IQuizRepository : IBaseRepository<Quiz>
    {
        Task<Quiz?> GetByQuizIdAsync(int quizId);
    }
}
