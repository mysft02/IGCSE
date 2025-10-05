using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly IGCSEContext _context;

        public CourseRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Course?> GetByCourseIdAsync(int courseId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Category)
                .Where(c => c.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByStatusAsync(CourseStatus status)
        {
            return await _context.Set<Course>()
                .Include(c => c.Category)
                .Where(c => c.Status == status)
                .ToListAsync();
        }
    }
}
