using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICoursesectionRepository : IBaseRepository<Coursesection>
    {
        Task<IEnumerable<Coursesection>> GetByCourseIdAsync(long courseId);
        Task<Coursesection?> GetByCourseSectionIdAsync(long courseSectionId);
        Task<IEnumerable<Coursesection>> GetActiveSectionsByCourseAsync(long courseId);
    }
}
