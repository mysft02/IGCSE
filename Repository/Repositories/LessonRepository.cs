using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        private readonly IGCSEContext _context;

        public LessonRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lesson>> GetByCourseSectionIdAsync(long courseSectionId)
        {
            return await _context.Set<Lesson>()
                .Include(l => l.CourseSection)
                .Where(l => l.CourseSectionId == courseSectionId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<Lesson?> GetByLessonIdAsync(long lessonId)
        {
            return await _context.Set<Lesson>()
                .Include(l => l.CourseSection)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);
        }

        public async Task<IEnumerable<Lesson>> GetActiveLessonsBySectionAsync(long courseSectionId)
        {
            return await _context.Set<Lesson>()
                .Include(l => l.CourseSection)
                .Where(l => l.CourseSectionId == courseSectionId && l.IsActive == 1)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }
    }
}
