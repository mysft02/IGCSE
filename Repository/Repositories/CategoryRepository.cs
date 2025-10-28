using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly IGCSEContext _context;

        public CategoryRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Category?> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Set<Category>()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Set<Category>()
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithCoursesAsync()
        {
            return await _context.Set<Category>()
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}
