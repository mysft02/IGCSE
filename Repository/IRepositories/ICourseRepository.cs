using Repository.IBaseRepository;
using BusinessObject.Model;
using BusinessObject.DTOs.Response.Courses;
using Common.Constants;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response.Dashboards;

namespace Repository.IRepositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(int courseId, decimal score);
        Task<Course?> GetByCourseIdAsync(int courseId);
        Task<Course?> GetByCourseIdWithCategoryAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
        Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, CourseListQuery query, string userRole);
        Task<Dictionary<string, int>> GetCoursesSortedByStatus();
        Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string creatorAccountId);
        Task<CourseAnalyticsResponse> GetCourseAnalyticsAsync(int courseId);
        Task<CourseDetailWithoutProgressResponse> GetCourseDetailAsync(int courseId);
        Task<bool> CheckOwnedByLessonItemId(int lessonItemId, string userId);
        Task<SimilarCourseForStudentRequest> GetCourseAndFinalQuizResult(string userId);
        Task<IEnumerable<Course>> GetAllSimilarCoursesForStudentAsync(List<float> embeddingData);
        Task<List<ActivityCountResponse>> GetActivityForYear(string userId, int year);
        Task<TeacherAnalyticsResponse> GetAnalytics(string userId, BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<AdminAnalyticsResponse> GetAdminAnalytics();
        Task<BusinessObject.DTOs.Response.Dashboards.StudentLearningAbilityResponse> GetStudentLearningAbilityAsync(BusinessObject.Payload.Request.Filter.StudentLearningAbilityQueryRequest query);
        Task<BusinessObject.DTOs.Response.Dashboards.LessonCompletionResponse> GetLessonCompletionAsync(BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest query, string teacherId);
        Task<BusinessObject.DTOs.Response.Dashboards.TopAnalyticsResponse> GetTopAnalyticsAsync(string? teacherId = null, int limit = 10);
        Task<BusinessObject.DTOs.Response.Dashboards.StudentLearningProgressResponse> GetStudentLearningProgressAsync(string studentId, BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<BusinessObject.DTOs.Response.Dashboards.StudentCourseStatisticsResponse> GetStudentCourseStatisticsAsync(string studentId);
        Task<BusinessObject.DTOs.Response.Dashboards.CourseRecommendationResponse> GetCourseRecommendationsAsync(string studentId, int limit = 10);
        Task<BusinessObject.DTOs.Response.Dashboards.AdminOverviewResponse> GetAdminOverviewAsync();
        Task<BusinessObject.DTOs.Response.Dashboards.AdminUserStatisticsResponse> GetAdminUserStatisticsAsync(BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<BusinessObject.DTOs.Response.Dashboards.AdminCourseStatisticsResponse> GetAdminCourseStatisticsAsync(BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<BusinessObject.DTOs.Response.Dashboards.AdminRevenueStatisticsResponse> GetAdminRevenueStatisticsAsync(BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<BusinessObject.DTOs.Response.Dashboards.AdminEnrollmentStatisticsResponse> GetAdminEnrollmentStatisticsAsync(BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<BusinessObject.DTOs.Response.Dashboards.ParentChildLearningProgressResponse> GetParentChildLearningProgressAsync(string parentId, BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null);
        Task<BusinessObject.DTOs.Response.Dashboards.ParentChildCourseStatisticsResponse> GetParentChildCourseStatisticsAsync(string parentId);
        Task<BusinessObject.DTOs.Response.Dashboards.ParentChildPerformanceResponse> GetParentChildPerformanceAsync(string parentId);
        Task<BusinessObject.DTOs.Response.Dashboards.ParentChildCertificateResponse> GetParentChildCertificatesAsync(string parentId);
    }
}
