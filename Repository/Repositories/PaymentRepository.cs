using BusinessObject;
using BusinessObject.DTOs.Response.Payment;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;
namespace Repository.Repositories
{
    public class PaymentRepository : BaseRepository<Transactionhistory>, IPaymentRepository
    {
        private readonly IGCSEContext _context;

        public PaymentRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, PaymentSummary>> GetPaymentSortedByDate()
        {
            var result = _context.Transactionhistories
                .AsEnumerable()
                .GroupBy(c => DateTime.ParseExact(c.VnpTransactionDate, "yyyyMMddHHmmss", null).ToString("yyyy-MM-dd"))
                .ToDictionary(
                g => g.Key,
                g => new PaymentSummary
                {
                    TotalAmount = g.Sum(x => x.Amount),
                    TotalCount = g.Count(),
                }
            );
            return result;
        }
    }
}
