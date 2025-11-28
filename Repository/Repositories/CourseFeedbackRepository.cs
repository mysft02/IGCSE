using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq;

namespace Repository.Repositories
{
    public class CourseFeedbackRepository : BaseRepository<Coursefeedback>, ICourseFeedbackRepository
    {
        private readonly IGCSEContext _context;

        public CourseFeedbackRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coursefeedback>> GetByCourseIdAsync(long courseId)
        {
            return await _context.Coursefeedbacks
                .Include(f => f.Student)
                .Where(f => f.CourseId == courseId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Coursefeedback?> GetByCourseAndStudentAsync(long courseId, string studentId)
        {
            return await _context.Coursefeedbacks
                .FirstOrDefaultAsync(f => f.CourseId == courseId && f.StudentId == studentId);
        }

        public async Task<double> GetAverageRatingAsync(long courseId)
        {
            return await _context.Coursefeedbacks
                .Where(f => f.CourseId == courseId)
                .Select(f => (double)f.Rating)
                .DefaultIfEmpty(0)
                .AverageAsync();
        }

        public async Task<int> GetFeedbackCountAsync(long courseId)
        {
            return await _context.Coursefeedbacks
                .CountAsync(f => f.CourseId == courseId);
        }
    }
}

