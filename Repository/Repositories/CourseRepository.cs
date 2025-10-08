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
    }
}
