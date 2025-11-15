using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IPackageRepository : IBaseRepository<Package>
    {
        Task<Package> GetByUserId(string userId);
        Task<Package> AddUserPackageAsync(Userpackage userPackage);
    }
}
