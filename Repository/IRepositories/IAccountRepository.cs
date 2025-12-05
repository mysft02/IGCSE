using BusinessObject.DTOs.Request.Accounts;
using BusinessObject.Model;
using Repository.IBaseRepository;

namespace Repository.IRepositories
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<(int totalAccount, int studentsAccount, int parentsAccount, int teachersAccount, int adminAccount)> GetTotalAccount();
        Task<Account> GetByAccountIdAsync(string accountId);
        string SendEmail(string recipientEmail, string subject, string htmlBody);
        string SendVerificationEmail(string email, string emailCode);
        Task<(List<Account> items, int totalCount, int page, int size)> GetPagedUserList(AccountListQuery query);
    }
}
