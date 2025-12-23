namespace BusinessObject.Model;

public class Notification
{
    public int NotificationId { get; set; }

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    /// <summary>
    /// Loại notification: COURSE_DISABLED, COURSE_APPROVED, COURSE_REJECTED, ...
    /// </summary>
    public string Type { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Loại entity liên quan: COURSE, REPORT, ...
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// ID của entity liên quan (ví dụ: CourseID)
    /// </summary>
    public int? RelatedEntityId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
