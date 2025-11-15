using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IStudentEnrollmentRepository : IBaseRepository<Studentenrollment>
    {
        Task<IEnumerable<Studentenrollment>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Studentenrollment>> GetByCourseIdAsync(int courseId);
        Task<Studentenrollment?> GetByStudentAndCourseAsync(string studentId, int courseId);
        Task<bool> IsStudentEnrolledAsync(string studentId, int courseId);
        Task<IEnumerable<Studentenrollment>> GetByParentIdAsync(string parentId);
    }
}


