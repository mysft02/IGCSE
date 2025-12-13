using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories;

public class ChatMessageRepository : BaseRepository<ChatMessage>, IChatMessageRepository
{
    private readonly IGCSEContext _context;

    public ChatMessageRepository(IGCSEContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ChatMessage>> GetByConversationIdAsync(int conversationId, int limit = 20)
    {
        return await _context.Set<ChatMessage>()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetRecentMessagesAsync(string userId, int limit = 5)
    {
        return await _context.Set<ChatMessage>()
            .Include(m => m.Conversation)
            .Where(m => m.Conversation != null && m.Conversation.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}

