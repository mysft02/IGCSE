using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories;

public class ChatConversationRepository : BaseRepository<ChatConversation>, IChatConversationRepository
{
    private readonly IGCSEContext _context;

    public ChatConversationRepository(IGCSEContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ChatConversation>> GetByUserIdAsync(string userId)
    {
        return await _context.Set<ChatConversation>()
            .Where(c => c.UserId == userId && c.IsActive == true)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatConversation?> GetActiveConversationAsync(string userId)
    {
        return await _context.Set<ChatConversation>()
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<ChatConversation?> GetByIdWithMessagesAsync(int conversationId, int messageLimit = 20)
    {
        return await _context.Set<ChatConversation>()
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(messageLimit))
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
    }
}

