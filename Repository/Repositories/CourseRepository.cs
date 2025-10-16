using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly IGCSEContext _context;

        public CourseRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Course?> GetByCourseIdAsync(long courseId)
        {
            return await _context.Set<Course>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course?> GetByCourseIdWithCategoryAsync(long courseId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(long categoryId)
        {
            return await _context.Set<Course>()
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, long? couseId, string? status)
        {
            var query = _context.Set<Course>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchByName))
            {
                var keyword = searchByName.Trim();
                query = query.Where(c => c.Name.Contains(keyword));
            }

            if (couseId.HasValue)
            {
                query = query.Where(c => c.CourseId == couseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            var total = await query.CountAsync();

            var skip = (page <= 1 ? 0 : (page - 1) * pageSize);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
