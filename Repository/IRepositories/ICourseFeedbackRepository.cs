using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICourseFeedbackRepository : IBaseRepository<Coursefeedback>
    {
        Task<IEnumerable<Coursefeedback>> GetByCourseIdAsync(long courseId);
        Task<Coursefeedback?> GetByCourseAndStudentAsync(long courseId, string studentId);
        Task<double> GetAverageRatingAsync(long courseId);
        Task<int> GetFeedbackCountAsync(long courseId);
    }
}

