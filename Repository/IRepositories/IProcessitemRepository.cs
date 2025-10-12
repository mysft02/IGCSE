using BusinessObject.Model;
using Repository.IBaseRepository;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IProcessitemRepository : IBaseRepository<Processitem>
    {
        Task<IEnumerable<Processitem>> GetByProcessIdAsync(long processId);
        Task<Processitem?> GetByProcessAndLessonItemAsync(long processId, long lessonItemId);
        Task<bool> IsLessonItemCompletedAsync(long processId, long lessonItemId);
    }
}
