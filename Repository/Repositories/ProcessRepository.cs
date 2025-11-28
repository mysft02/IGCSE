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

        public async Task<Process> UnlockLessonForStudentAsync(string studentId, int lessonId, int courseId)
        {
            // Kiểm tra xem Process đã tồn tại chưa
            var existingProcess = await GetByStudentAndLessonAsync(studentId, lessonId);
            
            if (existingProcess != null)
            {
                // Nếu đã tồn tại, chỉ cần update IsUnlocked
                existingProcess.IsUnlocked = true;
                existingProcess.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existingProcess;
            }
            
            // Nếu chưa tồn tại, tạo mới Process
            var newProcess = new Process
            {
                StudentId = studentId,
                LessonId = lessonId,
                CourseId = courseId,
                IsUnlocked = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _context.Set<Process>().AddAsync(newProcess);
            await _context.SaveChangesAsync();
            
            return newProcess;
        }

        public async Task<bool> HasStudentCompletedCourseAsync(string studentId, long courseId)
        {
            var totalLessons = await _context.Set<Lesson>()
                .Where(l => l.CourseSection.CourseId == courseId && l.IsActive == 1)
                .CountAsync();

            if (totalLessons == 0)
            {
                return false;
            }

            var lessonCompletion = await _context.Set<Process>()
                .Where(p => p.StudentId == studentId && p.CourseId == courseId)
                .Select(p => new
                {
                    p.LessonId,
                    LessonItemCount = p.Lesson.Lessonitems.Count,
                    CompletedItemCount = p.Processitems.Count
                })
                .ToListAsync();

            var completedLessons = lessonCompletion
                .Where(lc => lc.LessonItemCount == 0 || lc.CompletedItemCount >= lc.LessonItemCount)
                .Select(lc => lc.LessonId)
                .Distinct()
                .Count();

            return completedLessons == totalLessons;
        }
    }
}
