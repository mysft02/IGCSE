using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class ProcessRepository : BaseRepository<Process>, IProcessRepository
    {
        private readonly IGCSEContext _context;

        public ProcessRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Process?> GetByCourseKeyAndLessonAsync(long courseKeyId, long lessonId)
        {
            return await _context.Set<Process>()
                .Include(p => p.CourseKey)
                .Include(p => p.Lesson)
                .FirstOrDefaultAsync(p => p.CourseKeyId == courseKeyId && p.LessonId == lessonId);
        }

        public async Task<IEnumerable<Process>> GetByCourseKeyAsync(long courseKeyId)
        {
            return await _context.Set<Process>()
                .Include(p => p.CourseKey)
                .Include(p => p.Lesson)
                .Where(p => p.CourseKeyId == courseKeyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Process>> GetByLessonAsync(long lessonId)
        {
            return await _context.Set<Process>()
                .Include(p => p.CourseKey)
                .Include(p => p.Lesson)
                .Where(p => p.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<bool> IsLessonCompletedAsync(long courseKeyId, long lessonId)
        {
            var process = await GetByCourseKeyAndLessonAsync(courseKeyId, lessonId);
            if (process == null) return false;

            // Check if all lesson items are completed
            var lessonItems = await _context.Set<Lessonitem>()
                .Where(li => li.LessonId == lessonId)
                .ToListAsync();

            var completedItems = await _context.Set<Processitem>()
                .Where(pi => pi.ProcessId == process.ProcessId)
                .ToListAsync();

            return lessonItems.Count == completedItems.Count;
        }
    }
}
