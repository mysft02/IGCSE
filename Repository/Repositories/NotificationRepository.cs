using BusinessObject;
using BusinessObject.DTOs.Request.Notifications;
using BusinessObject.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationRepository(
            IGCSEContext context,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<(IEnumerable<Notification> items, int total)> GetNotificationsPagedAsync(string userId, NotificationQueryRequest request)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .AsQueryable();

            // Filter by read status
            if (request.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == request.IsRead.Value);
            }

            // Filter by type
            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                query = query.Where(n => n.Type == request.Type);
            }

            // Filter by related entity type
            if (!string.IsNullOrWhiteSpace(request.RelatedEntityType))
            {
                query = query.Where(n => n.RelatedEntityType == request.RelatedEntityType);
            }

            // Search by title
            if (!string.IsNullOrWhiteSpace(request.SearchByTitle))
            {
                query = query.Where(n => n.Title.Contains(request.SearchByTitle));
            }

            // Order by created date (newest first)
            query = query.OrderByDescending(n => n.CreatedAt);

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
