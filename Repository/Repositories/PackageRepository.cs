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

        public async Task<bool> CheckDuplicate(int packageId, string userId)
        {
            var package = _context.Userpackages
                .FirstOrDefault(p => p.PackageId == packageId && p.UserId == userId);

            if (package != null)
            {
                return true;
            }
            return false;
        }

        public async Task<Package?> AddUserPackageAsync(Userpackage userpackage)
        {
            var result = _context.Userpackages.Add(userpackage);
            await _context.SaveChangesAsync();
            return userpackage.Package;
        }

        public async Task<Package?> GetByUserId(string userId)
        {
            var package = _context.Packages
                .FirstOrDefault(c => c.Userpackages.Any(p => p.UserId == userId && p.IsActive == true));

            return package;
        }
    }
}
