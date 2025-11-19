using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.Model;
using Repository.IBaseRepository;
using System.Linq.Expressions;

namespace Repository.IRepositories
{
    public interface IStudentEnrollmentRepository : IBaseRepository<Studentenrollment>
    {
        Task<IEnumerable<Studentenrollment>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Studentenrollment>> GetByCourseIdAsync(int courseId);
        Task<Studentenrollment?> GetByStudentAndCourseAsync(string studentId, int courseId);
        Task<bool> IsStudentEnrolledAsync(string studentId, int courseId);
        Task<IEnumerable<Studentenrollment>> GetByParentIdAsync(string parentId);

        Task<List<ParentEnrollmentResponse>> GetListBoughtCourses(ParentEnrollmentQueryRequest request, Expression<Func<Studentenrollment, bool>>? filter = null);
    }
}


