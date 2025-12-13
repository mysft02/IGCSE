namespace BusinessObject.DTOs.Response.Assistant;

public class ConversationResponse
{
    public int ConversationId { get; set; }
    public string? Title { get; set; }
    public string? PersonaType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

