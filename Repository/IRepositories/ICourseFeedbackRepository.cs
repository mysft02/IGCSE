using BusinessObject.DTOs.Request.Courses;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICourseFeedbackRepository : IBaseRepository<Coursefeedback>
    {
        Task<IEnumerable<Coursefeedback>> GetByCourseIdAsync(int courseId);
        Task<Coursefeedback?> GetByCourseAndStudentAsync(int courseId, string studentId);
        Task<double> GetAverageRatingAsync(int courseId);
        Task<int> GetFeedbackCountAsync(int courseId);
        Task<(IEnumerable<Coursefeedback> items, int total)> GetFeedbacksPagedAsync(int courseId, CourseFeedbackQueryRequest request);
    }
}

