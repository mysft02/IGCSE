namespace BusinessObject.DTOs.Response.Notifications
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }

        public string UserId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string Type { get; set; } = null!;

        public bool IsRead { get; set; }

        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
