using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ILessonRepository : IBaseRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetByCourseSectionIdAsync(long courseSectionId);
        Task<Lesson?> GetByLessonIdAsync(long lessonId);
        Task<IEnumerable<Lesson>> GetActiveLessonsBySectionAsync(long courseSectionId);
        Task<Lesson?> GetNextLessonAsync(int currentLessonId);
    }
}
