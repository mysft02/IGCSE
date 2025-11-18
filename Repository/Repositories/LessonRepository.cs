using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

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
                .Include(l => l.Quiz)
                .Where(l => l.CourseSectionId == courseSectionId && l.IsActive == 1)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<Lesson?> GetNextLessonAsync(int currentLessonId)
        {
            // Lấy lesson hiện tại để biết section và order
            var currentLesson = await _context.Set<Lesson>()
                .Include(l => l.CourseSection)
                .FirstOrDefaultAsync(l => l.LessonId == currentLessonId);

            if (currentLesson == null) return null;

            // Tìm lesson tiếp theo trong cùng section
            var nextLessonInSection = await _context.Set<Lesson>()
                .Include(l => l.CourseSection)
                .Where(l => l.CourseSectionId == currentLesson.CourseSectionId 
                    && l.Order > currentLesson.Order 
                    && l.IsActive == 1)
                .OrderBy(l => l.Order)
                .FirstOrDefaultAsync();

            if (nextLessonInSection != null)
            {
                return nextLessonInSection;
            }

            // Nếu không có lesson tiếp theo trong section, tìm lesson đầu tiên của section tiếp theo
            var currentSection = currentLesson.CourseSection;
            var nextSection = await _context.Set<Coursesection>()
                .Where(cs => cs.CourseId == currentSection.CourseId 
                    && cs.Order > currentSection.Order 
                    && cs.IsActive == 1)
                .OrderBy(cs => cs.Order)
                .FirstOrDefaultAsync();

            if (nextSection != null)
            {
                return await _context.Set<Lesson>()
                    .Include(l => l.CourseSection)
                    .Where(l => l.CourseSectionId == nextSection.CourseSectionId && l.IsActive == 1)
                    .OrderBy(l => l.Order)
                    .FirstOrDefaultAsync();
            }

            return null; // Không có lesson tiếp theo (đã hoàn thành khóa học)
        }
    }
}
