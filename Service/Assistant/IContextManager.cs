namespace Service.Assistant;

public class ChatContext
{
    public string UserId { get; set; } = null!;
    public string UserRole { get; set; } = null!;
    public int? CurrentCourseId { get; set; }
    public string? CurrentCourseName { get; set; }
    public decimal? LearningProgress { get; set; }
    public List<string> RecentMessages { get; set; } = new();
    public string? SystemDataContext { get; set; } // Dữ liệu hệ thống động
}

public interface IContextManager
{
    Task<ChatContext> BuildContextAsync(string userId, string? userRole = null, string? userQuery = null);
}

