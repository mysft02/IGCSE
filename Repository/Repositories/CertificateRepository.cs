using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CertificateRepository : BaseRepository<Certificate>, ICertificateRepository
    {
        private readonly IGCSEContext _context;
        public CertificateRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
