using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;
using System;
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
