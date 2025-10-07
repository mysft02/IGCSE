using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<Course?> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
    }
}
