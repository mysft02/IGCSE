using BusinessObject.Model;
using DataAccessObject;
using MailKit.Net.Smtp;
using MimeKit;
using Repository.BaseRepository;
using Repository.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        private readonly AccountDAO _accountDao;

        public AccountRepository(AccountDAO accountDao) : base(accountDao)
        {
            _accountDao = accountDao;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _accountDao.GetAllAsync();
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            return await _accountDao.GetByIdAsync(id);
        }

        public async Task<Account> GetByStringIdAsync(string id)
        {
            return await _accountDao.GetByStringIdAsync(id);
        }

        public async Task<Account> AddAsync(Account entity)
        {
            return await _accountDao.AddAsync(entity);
        }

        public async Task<Account> UpdateAsync(Account entity)
        {
            return await _accountDao.UpdateAsync(entity);
        }

        public async Task<Account> DeleteAsync(Account entity)
        {
            return await _accountDao.DeleteAsync(entity);
        }

        public async Task<(int totalAccount, int managersAccount, int customersAccount, int staffsAccount, int consultantAccount)> GetTotalAccount()
        {
            return await _accountDao.GetTotalAccount();
        }

        public async Task<Account> GetByAccountIdAsync(string accountId)
        {
            return await _accountDao.GetByStringIdAsync(accountId);
        }

        public string SendEmail(string recipientEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.From.Add(MailboxAddress.Parse("huydqse173363@fpt.edu.vn"));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("huydqse173363@fpt.edu.vn", "hmkf wdjs epwx ibfg");
            smtp.Send(message);
            smtp.Disconnect(true);

            return "Email sent successfully";
        }

    }
}
