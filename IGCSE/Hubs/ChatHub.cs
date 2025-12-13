using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service.Assistant;
using Repository.IRepositories;
using BusinessObject.Model;
using System.Security.Claims;

namespace IGCSE.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IAssistantService _assistantService;
    private readonly IChatMessageRepository _messageRepository;

    public ChatHub(
        IAssistantService assistantService,
        IChatMessageRepository messageRepository)
    {
        _assistantService = assistantService;
        _messageRepository = messageRepository;
    }

    public async Task SendMessage(string conversationId, string message)
    {
        var userId = Context.User?.FindFirst("AccountID")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        // Save user message
        if (int.TryParse(conversationId, out var convId))
        {
            var userMessage = new ChatMessage
            {
                ConversationId = convId,
                Content = message,
                Role = "user",
                CreatedAt = DateTime.UtcNow,
                IsRelevant = true
            };
            await _messageRepository.AddAsync(userMessage);
        }

        // Notify typing
        await Clients.Group(conversationId).SendAsync("Typing", true);

        try
        {
            // Process with AI (streaming)
            await foreach (var chunk in _assistantService.ProcessMessageStreamAsync(
                userId, message, conversationId))
            {
                await Clients.Caller.SendAsync("ReceiveMessageChunk", chunk);
            }
        }
        catch (Exception ex)
        {
            // Log error for debugging
            Console.WriteLine($"Error in ChatHub.SendMessage: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            await Clients.Caller.SendAsync("Error", $"Lỗi: {ex.Message}");
        }
        finally
        {
            // Stop typing
            await Clients.Group(conversationId).SendAsync("Typing", false);
        }
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }
}

