using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class PaymentInformationRepository : BaseRepository<Paymentinformation>, IPaymentInformationRepository
    {
        private IGCSEContext _context;

        public PaymentInformationRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
