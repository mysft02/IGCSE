using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository;
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
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Set<Category>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithCoursesAsync()
        {
            return await _context.Set<Category>()
                .Include(c => c.Courses)
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}
