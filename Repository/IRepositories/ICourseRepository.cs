using Repository.IBaseRepository;
using BusinessObject.Model;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response.Courses;
using System.Linq.Expressions;
using Common.Constants;

namespace Repository.IRepositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(int courseId, decimal score);
        Task<Course?> GetByCourseIdAsync(int courseId);
        Task<Course?> GetByCourseIdWithCategoryAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
        Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, int? couseId, CourseStatusEnum? status);
        Task<Dictionary<string, int>> GetCoursesSortedByStatus();
        Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string creatorAccountId);
        Task<CourseAnalyticsResponse> GetCourseAnalyticsAsync(int courseId);
        Task<CourseDetailWithoutProgressResponse> GetCourseDetailAsync(int courseId);
        Task<bool> CheckOwnedByLessonItemId(int lessonItemId, string userId);
    }
}
