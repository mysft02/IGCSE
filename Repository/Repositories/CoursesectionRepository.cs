using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class CoursesectionRepository : BaseRepository<Coursesection>, ICoursesectionRepository
    {
        private readonly IGCSEContext _context;

        public CoursesectionRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coursesection>> GetByCourseIdAsync(long courseId)
        {
            return await _context.Set<Coursesection>()
                .Include(cs => cs.Course)
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.Order)
                .ToListAsync();
        }

        public async Task<Coursesection?> GetByCourseSectionIdAsync(long courseSectionId)
        {
            return await _context.Set<Coursesection>()
                .Include(cs => cs.Course)
                .FirstOrDefaultAsync(cs => cs.CourseSectionId == courseSectionId);
        }

        public async Task<IEnumerable<Coursesection>> GetActiveSectionsByCourseAsync(long courseId)
        {
            return await _context.Set<Coursesection>()
                .Include(cs => cs.Course)
                .Where(cs => cs.CourseId == courseId && cs.IsActive == 1)
                .OrderBy(cs => cs.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Coursesection>> GetByChapterIdAsync(int chapterId)
        {
            return await _context.Coursesections.Where(s => s.ChapterId == chapterId).ToListAsync();
        }
    }
}
