using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IPackageRepository : IBaseRepository<Package>
    {
        Task<bool> CheckDuplicate(int packageId, string userId);
        Task<Package> GetByUserId(string userId);
        Task<Package> AddUserPackageAsync(Userpackage userPackage);
    }
}
