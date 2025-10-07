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

        public async Task<Course?> GetByCourseIdAsync(int courseId)
        {
            return await _context.Set<Course>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Set<Course>()
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}
