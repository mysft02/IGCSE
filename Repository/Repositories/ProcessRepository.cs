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

        public async Task<IEnumerable<Process>> GetByLessonAsync(long lessonId)
        {
            return await _context.Set<Process>()
                .Include(p => p.Lesson)
                .Where(p => p.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<Process?> GetByStudentAndLessonAsync(string studentId, long lessonId)
        {
            return await _context.Set<Process>()
                .Include(p => p.Lesson)
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);
        }

        public async Task<IEnumerable<Process>> GetByStudentAndCourseAsync(string studentId, long courseId)
        {
            return await _context.Set<Process>()
                .Include(p => p.Lesson)
                .Where(p => p.StudentId == studentId && p.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<bool> IsLessonCompletedForStudentAsync(string studentId, long lessonId)
        {
            var process = await GetByStudentAndLessonAsync(studentId, lessonId);
            if (process == null) return false;

            var lessonItems = await _context.Set<Lessonitem>()
                .Where(li => li.LessonId == lessonId)
                .ToListAsync();

            var completedItems = await _context.Set<Processitem>()
                .Where(pi => pi.ProcessId == process.ProcessId)
                .ToListAsync();

            return lessonItems.Count == completedItems.Count;
        }

        public async Task<IEnumerable<Process>> GetByStudentAsync(string studentId)
        {
            return await _context.Set<Process>()
                .Include(p => p.Lesson)
                .Where(p => p.StudentId == studentId)
                .ToListAsync();
        }
    }
}
