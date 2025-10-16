using Repository.IBaseRepository;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(long courseId, decimal score);
        Task<Course?> GetByCourseIdAsync(long courseId);
        Task<Course?> GetByCourseIdWithCategoryAsync(long courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(long categoryId);
    }
}
