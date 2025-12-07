using BusinessObject.Model;
using BusinessObject.Enums;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IModuleRepository : IBaseRepository<Module>
    {
        Task<IEnumerable<Module>> GetAllAsync();
        Task<(IEnumerable<Module> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, CourseSubject? courseSubject, bool? isActive);
        Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Module>> GetByCourseSubjectAsync(CourseSubject courseSubject);
        Task<Module?> GetByIdAsync(int moduleId);
        Task<Module> AddAsync(Module module);
        Task<Module> UpdateAsync(Module module);
        Task DeleteAsync(int moduleId);
    }
}
