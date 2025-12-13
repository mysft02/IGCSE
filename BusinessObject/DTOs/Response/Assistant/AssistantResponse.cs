namespace BusinessObject.DTOs.Response.Assistant;

public class AssistantResponse
{
    public string Content { get; set; } = null!;
    public bool IsRelevant { get; set; } = true;
    public string? ConversationId { get; set; }
    public List<int>? RelevantKnowledgeIds { get; set; }
}

