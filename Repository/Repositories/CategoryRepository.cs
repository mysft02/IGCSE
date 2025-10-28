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

        public async Task<(IEnumerable<Category> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, bool? isActive)
        {
            var query = _context.Set<Category>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchByName))
            {
                var keyword = searchByName.Trim();
                query = query.Where(c => c.CategoryName.Contains(keyword));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var total = await query.CountAsync();
            var skip = (page <= 1 ? 0 : (page - 1) * pageSize);
            var items = await query
                .OrderBy(c => c.CategoryName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
