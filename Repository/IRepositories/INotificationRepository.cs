using BusinessObject.DTOs.Request.Notifications;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<(IEnumerable<Notification> items, int total)> GetNotificationsPagedAsync(string userId, NotificationQueryRequest request);
    }
}
