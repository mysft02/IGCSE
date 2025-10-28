using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IProcessRepository : IBaseRepository<Process>
    {
        Task<Process?> GetByCourseKeyAndLessonAsync(long courseKeyId, long lessonId);
        Task<IEnumerable<Process>> GetByCourseKeyAsync(long courseKeyId);
        Task<IEnumerable<Process>> GetByLessonAsync(long lessonId);
        Task<bool> IsLessonCompletedAsync(long courseKeyId, long lessonId);
    }
}
