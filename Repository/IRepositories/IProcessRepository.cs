using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IProcessRepository : IBaseRepository<Process>
    {
        Task<IEnumerable<Process>> GetByLessonAsync(long lessonId);
        // New student-based APIs (no courseKey)
        Task<Process?> GetByStudentAndLessonAsync(string studentId, long lessonId);
        Task<IEnumerable<Process>> GetByStudentAndCourseAsync(string studentId, long courseId);
        Task<bool> IsLessonCompletedForStudentAsync(string studentId, long lessonId);
        Task<IEnumerable<Process>> GetByStudentAsync(string studentId);
    }
}
