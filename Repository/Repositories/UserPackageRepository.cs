using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class UserPackageRepository : BaseRepository<Userpackage>, IUserPackageRepository
    {
        private readonly IGCSEContext _context;

        public UserPackageRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
