using BusinessObject;
using BusinessObject.DTOs.Response.Payment;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Globalization;
namespace Repository.Repositories
{
    public class PaymentRepository : BaseRepository<Transactionhistory>, IPaymentRepository
    {
        private readonly IGCSEContext _context;

        public PaymentRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
