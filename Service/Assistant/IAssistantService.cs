using BusinessObject.DTOs.Response.Assistant;
using BusinessObject.Model;

namespace Service.Assistant;

public interface IAssistantService
{
    Task<AssistantResponse> ProcessMessageAsync(
        string userId,
        string message,
        string? conversationId = null);

    IAsyncEnumerable<string> ProcessMessageStreamAsync(
        string userId,
        string message,
        string? conversationId = null);

    Task<string> CreateConversationAsync(
        string userId,
        string? initialMessage = null);

    Task<List<ChatMessage>> GetConversationHistoryAsync(
        string conversationId,
        int limit = 20);
}

