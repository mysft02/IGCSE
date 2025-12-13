using Repository.IBaseRepository;
using BusinessObject.Model;

namespace Repository.IRepositories;

public interface IChatConversationRepository : IBaseRepository<ChatConversation>
{
    Task<List<ChatConversation>> GetByUserIdAsync(string userId);
    Task<ChatConversation?> GetActiveConversationAsync(string userId);
    Task<ChatConversation?> GetByIdWithMessagesAsync(int conversationId, int messageLimit = 20);
}

