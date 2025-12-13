using Repository.IBaseRepository;
using BusinessObject.Model;

namespace Repository.IRepositories;

public interface IChatMessageRepository : IBaseRepository<ChatMessage>
{
    Task<List<ChatMessage>> GetByConversationIdAsync(int conversationId, int limit = 20);
    Task<List<ChatMessage>> GetRecentMessagesAsync(string userId, int limit = 5);
}

