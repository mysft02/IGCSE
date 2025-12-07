using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IProcessRepository : IBaseRepository<Process>
    {
        Task<IEnumerable<Process>> GetByLessonAsync(int lessonId);
        Task<Process?> GetByStudentAndLessonAsync(string studentId, int lessonId);
        Task<IEnumerable<Process>> GetByStudentAndCourseAsync(string studentId, int courseId);
        Task<bool> IsLessonCompletedForStudentAsync(string studentId, int lessonId);
        Task<IEnumerable<Process>> GetByStudentAsync(string studentId);
        Task<Process> UnlockLessonForStudentAsync(string studentId, int lessonId, int courseId);
        Task<bool> HasStudentCompletedCourseAsync(string studentId, int courseId);
    }
}
