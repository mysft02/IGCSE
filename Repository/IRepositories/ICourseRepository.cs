using Repository.IBaseRepository;
using BusinessObject.Model;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response.Courses;
using System.Linq.Expressions;

namespace Repository.IRepositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(long courseId, decimal score);
        Task<Course?> GetByCourseIdAsync(long courseId);
        Task<Course?> GetByCourseIdWithCategoryAsync(long courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(long categoryId);
        Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, long? couseId, string? status);
        Task<Dictionary<string, int>> GetCoursesSortedByStatus();
        Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string creatorAccountId);
        Task<CourseAnalyticsResponse> GetCourseAnalyticsAsync(int courseId);
        Task<CourseDetailWithoutProgressResponse> GetCourseDetailAsync(int courseId);
        Task<bool> CheckOwnedByLessonItemId(int lessonItemId, string userId);
    }
}
