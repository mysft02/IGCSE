using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface ITrelloTokenRepository : IBaseRepository<TrelloToken>
    {
        Task<List<TrelloToken>> GetByUserIdAsync(string userId);
        Task<TrelloToken?> GetByTrelloIdAsync(string trelloId);
    }
}
