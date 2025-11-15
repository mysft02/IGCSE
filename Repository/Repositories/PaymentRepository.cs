using BusinessObject;
using BusinessObject.DTOs.Response.Payment;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public class PaymentRepository : BaseRepository<Transactionhistory>, IPaymentRepository
    {
        private readonly IGCSEContext _context;

        public PaymentRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Transactionhistory?> GetByUserAndCourseAsync(string userId, int courseId)
        {
            return await _context.Transactionhistories
                .FirstOrDefaultAsync(t => t.UserId == userId && t.ItemId == courseId);
        }
    }
}
