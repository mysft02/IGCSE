using BusinessObject;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CoursereportRepository : BaseRepository<Coursereport>, ICoursereportRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CoursereportRepository(
            IGCSEContext context,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Coursereport>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Coursereports
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Coursereport>> GetByUserIdAsync(string userId)
        {
            return await _context.Coursereports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Coursereport>> GetByStatusAsync(string status)
        {
            return await _context.Coursereports
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Coursereport> items, int total)> GetReportsPagedAsync(int courseId, CourseReportQueryRequest request)
        {
            var query = _context.Coursereports
                .Where(r => r.CourseId == courseId)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(r => r.Status == request.Status);
            }

            // Filter by report type
            if (!string.IsNullOrWhiteSpace(request.ReportType))
            {
                query = query.Where(r => r.ReportType != null && r.ReportType.Contains(request.ReportType));
            }

            // Search by title
            if (!string.IsNullOrWhiteSpace(request.SearchByTitle))
            {
                query = query.Where(r => r.Title.Contains(request.SearchByTitle));
            }

            // Order by created date (newest first)
            query = query.OrderByDescending(r => r.CreatedAt);

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

