using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ILessonitemRepository : IBaseRepository<Lessonitem>
    {
        Task<IEnumerable<Lessonitem>> GetByLessonIdAsync(int lessonId);
        Task<Lessonitem?> GetByLessonItemIdAsync(int lessonItemId);
    }
}
