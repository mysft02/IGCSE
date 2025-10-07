
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesWithCoursesAsync();
    }
}
