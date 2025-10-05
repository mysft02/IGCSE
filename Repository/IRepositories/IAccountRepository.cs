using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<(int totalAccount, int studentsAccount, int parentsAccount, int teachersAccount, int adminAccount)> GetTotalAccount();
        Task<Account> GetByAccountIdAsync(string accountId);
        string SendEmail(string recipientEmail, string subject, string htmlBody);
    }
}
