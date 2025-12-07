using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class PayoutHistoryRepository : BaseRepository<Payouthistory>, IPayoutHistoryRepository
    {
        private readonly IGCSEContext _context;

        public PayoutHistoryRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
