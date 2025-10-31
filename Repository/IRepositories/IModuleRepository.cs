using BusinessObject.Model;

namespace Repository.IRepositories
{
    public interface IModuleRepository
    {
        Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId);
        Task<Module?> GetByIdAsync(int moduleId);
        Task<Module> AddAsync(Module module);
        Task<Module> UpdateAsync(Module module);
        Task DeleteAsync(int moduleId);
    }
}
