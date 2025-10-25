using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class TrelloTokenRepository : BaseRepository<TrelloToken>, ITrelloTokenRepository
    {
        private readonly IGCSEContext _context;

        public TrelloTokenRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<TrelloToken>> GetByUserIdAsync(string userId)
        {
            return await _context.TrelloTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<TrelloToken?> GetByTrelloIdAsync(string trelloId)
        {
            return await _context.TrelloTokens
                .FirstOrDefaultAsync(t => t.TrelloId == trelloId);
        }
    }
}
