using Repository.IBaseRepository;
using BusinessObject.Model;
using BusinessObject.DTOs.Response.Courses;
using Common.Constants;
using BusinessObject.DTOs.Request.Courses;

namespace Repository.IRepositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(int courseId, decimal score);
        Task<Course?> GetByCourseIdAsync(int courseId);
        Task<Course?> GetByCourseIdWithCategoryAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
        Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, CourseListQuery query);
        Task<Dictionary<string, int>> GetCoursesSortedByStatus();
        Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string creatorAccountId);
        Task<CourseAnalyticsResponse> GetCourseAnalyticsAsync(int courseId);
        Task<CourseDetailWithoutProgressResponse> GetCourseDetailAsync(int courseId);
        Task<bool> CheckOwnedByLessonItemId(int lessonItemId, string userId);
    }
}
