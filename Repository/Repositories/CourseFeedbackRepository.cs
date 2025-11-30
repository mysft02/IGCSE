using BusinessObject;
using BusinessObject.DTOs.Request.Courses;
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

        public async Task<IEnumerable<Coursefeedback>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Coursefeedbacks
                .Include(f => f.Student)
                .Where(f => f.CourseId == courseId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Coursefeedback?> GetByCourseAndStudentAsync(int courseId, string studentId)
        {
            return await _context.Coursefeedbacks
                .FirstOrDefaultAsync(f => f.CourseId == courseId && f.StudentId == studentId);
        }

        public async Task<double> GetAverageRatingAsync(int courseId)
        {
            return await _context.Coursefeedbacks
                .Where(f => f.CourseId == courseId)
                .Select(f => (double)f.Rating)
                .DefaultIfEmpty(0)
                .AverageAsync();
        }

        public async Task<int> GetFeedbackCountAsync(int courseId)
        {
            return await _context.Coursefeedbacks
                .CountAsync(f => f.CourseId == courseId);
        }

        public async Task<(IEnumerable<Coursefeedback> items, int total)> GetFeedbacksPagedAsync(int courseId, CourseFeedbackQueryRequest request)
        {
            var query = _context.Coursefeedbacks
                .Include(f => f.Student)
                .Where(f => f.CourseId == courseId)
                .AsQueryable();

            // Filter by rating
            if (request.Rating.HasValue && request.Rating.Value >= 1 && request.Rating.Value <= 5)
            {
                query = query.Where(f => f.Rating == request.Rating.Value);
            }

            // Filter by student name
            if (!string.IsNullOrWhiteSpace(request.SearchByStudentName))
            {
                query = query.Where(f => f.Student != null && 
                    (f.Student.Name != null && f.Student.Name.Contains(request.SearchByStudentName)) ||
                    (f.Student.UserName != null && f.Student.UserName.Contains(request.SearchByStudentName)));
            }

            // Apply sorting
            var sortBy = request.SortBy?.ToLower() ?? "date";
            var sortOrder = request.SortOrder?.ToLower() ?? "desc";

            if (sortBy == "rating")
            {
                query = sortOrder == "asc" 
                    ? query.OrderBy(f => f.Rating).ThenByDescending(f => f.CreatedAt)
                    : query.OrderByDescending(f => f.Rating).ThenByDescending(f => f.CreatedAt);
            }
            else // default: sort by date
            {
                query = sortOrder == "asc"
                    ? query.OrderBy(f => f.CreatedAt)
                    : query.OrderByDescending(f => f.CreatedAt);
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Apply pagination
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}

