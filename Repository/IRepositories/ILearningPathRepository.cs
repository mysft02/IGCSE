using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ILearningPathRepository : IBaseRepository<Learningpath>
    {
        Task<Learningpath> GetMatchLearningPath(List<int> moduleList);
    }
}
