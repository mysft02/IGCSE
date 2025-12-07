using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICoursesectionRepository : IBaseRepository<Coursesection>
    {
        Task<IEnumerable<Coursesection>> GetByCourseIdAsync(int courseId);
        Task<Coursesection?> GetByCourseSectionIdAsync(int courseSectionId);
        Task<IEnumerable<Coursesection>> GetActiveSectionsByCourseAsync(int courseId);
    }
}
