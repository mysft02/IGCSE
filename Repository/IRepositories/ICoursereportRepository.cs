using BusinessObject.DTOs.Request.Courses;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICoursereportRepository : IBaseRepository<Coursereport>
    {
        Task<IEnumerable<Coursereport>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Coursereport>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Coursereport>> GetByStatusAsync(string status);
        Task<(IEnumerable<Coursereport> items, int total)> GetReportsPagedAsync(int courseId, CourseReportQueryRequest request);
    }
}

