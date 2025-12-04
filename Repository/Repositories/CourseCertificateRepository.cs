using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CourseCertificateRepository : BaseRepository<Coursecertificate>, ICourseCertificateRepository
    {
        private readonly IGCSEContext _context;

        public CourseCertificateRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
