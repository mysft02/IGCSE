using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class StudentEnrollmentRepository : BaseRepository<Studentenrollment>, IStudentEnrollmentRepository
    {
        private readonly IGCSEContext _context;

        public StudentEnrollmentRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Studentenrollment>> GetByStudentIdAsync(string studentId)
        {
            return await _context.Studentenrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Studentenrollment>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Studentenrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<Studentenrollment?> GetByStudentAndCourseAsync(string studentId, int courseId)
        {
            return await _context.Studentenrollments
                .Include(e => e.Course)
                .Include(e => e.Parent)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<bool> IsStudentEnrolledAsync(string studentId, int courseId)
        {
            return await _context.Studentenrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<IEnumerable<Studentenrollment>> GetByParentIdAsync(string parentId)
        {
            return await _context.Studentenrollments
                .Where(e => e.ParentId == parentId)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }
    }
}

