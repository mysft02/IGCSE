using BusinessObject.Model;
using Repository.IBaseRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<(int totalAccount, int managersAccount, int customersAccount, int staffsAccount, int consultantAccount)> GetTotalAccount();
        Task<Account> GetByAccountIdAsync(string accountId);
        string SendEmail(string recipientEmail, string subject, string htmlBody);
    }
}
