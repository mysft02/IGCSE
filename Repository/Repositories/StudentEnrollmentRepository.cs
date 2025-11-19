using BusinessObject;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq.Expressions;

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

        public async Task<List<ParentEnrollmentResponse>> GetListBoughtCourses(ParentEnrollmentQueryRequest request, Expression<Func<Studentenrollment, bool>>? filter = null)
        {
            var query = _context.Studentenrollments
                .Include(x => x.Course)
                .AsQueryable();

            var resultList = await query
                .Where(x => x.ParentId == request.userID)
                .Select(p => new ParentEnrollmentResponse
                {
                    CourseId = p.CourseId,
                    CourseName = p.Course.Name,
                    CourseDescription = p.Course.Description,
                    ImageUrl = p.Course.ImageUrl,
                    EnrolledAt = p.EnrolledAt
                })
                .Distinct()
                .ToListAsync();

            return resultList;
        }
    }
}

