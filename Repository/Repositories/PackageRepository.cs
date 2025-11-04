using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class PackageRepository : BaseRepository<Package>, IPackageRepository
    {
        private readonly IGCSEContext _context;

        public PackageRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
