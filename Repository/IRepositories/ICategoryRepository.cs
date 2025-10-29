using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesWithCoursesAsync();
        Task<(IEnumerable<Category> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, bool? isActive);
    }
}
