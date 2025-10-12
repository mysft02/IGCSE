using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class CoursekeyRepository : BaseRepository<Coursekey>, ICoursekeyRepository
    {
        private readonly IGCSEContext _context;

        public CoursekeyRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Coursekey?> GetByCourseAndStudentAsync(long courseId, string studentId)
        {
            return await _context.Set<Coursekey>()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.StudentId == studentId);
        }

        public async Task<Coursekey?> GetByCourseKeyAsync(long courseKeyId)
        {
            return await _context.Set<Coursekey>()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CourseKeyId == courseKeyId);
        }

        public async Task<IEnumerable<Coursekey>> GetByStudentIdAsync(string studentId)
        {
            return await _context.Set<Coursekey>()
                .Include(c => c.Course)
                .Where(c => c.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Coursekey>> GetByCourseIdAsync(long courseId)
        {
            return await _context.Set<Coursekey>()
                .Where(c => c.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<string> GenerateUniqueCourseKeyAsync(long courseId, string studentId)
        {
            // Generate a unique course key based on course and student
            var courseKey = $"{courseId}-{studentId}-{DateTime.UtcNow.Ticks}";
            return courseKey;
        }
    }
}
