using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ILessonRepository : IBaseRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetByCourseSectionIdAsync(int courseSectionId);
        Task<Lesson?> GetByLessonIdAsync(int lessonId);
        Task<IEnumerable<Lesson>> GetActiveLessonsBySectionAsync(int courseSectionId);
        Task<Lesson?> GetNextLessonAsync(int currentLessonId);
    }
}
