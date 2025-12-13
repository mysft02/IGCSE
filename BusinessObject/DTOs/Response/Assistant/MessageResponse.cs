namespace BusinessObject.DTOs.Response.Assistant;

public class MessageResponse
{
    public int MessageId { get; set; }
    public string Content { get; set; } = null!;
    public string Role { get; set; } = null!; // "user" or "assistant"
    public DateTime CreatedAt { get; set; }
    public string? Metadata { get; set; }
    public bool IsRelevant { get; set; }
}

