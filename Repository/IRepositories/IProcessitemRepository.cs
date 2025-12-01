using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IProcessitemRepository : IBaseRepository<Processitem>
    {
        Task<IEnumerable<Processitem>> GetByProcessIdAsync(int processId);
        Task<Processitem?> GetByProcessAndLessonItemAsync(int processId, int lessonItemId);
        Task<bool> IsLessonItemCompletedAsync(int processId, int lessonItemId);
    }
}
