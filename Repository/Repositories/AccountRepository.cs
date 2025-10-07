
using BusinessObject;
using BusinessObject.Model;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        private readonly IGCSEContext _context;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountRepository(IGCSEContext context, UserManager<Account> userManager, RoleManager<IdentityRole> roleManager) : base(context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<(int totalAccount, int studentsAccount, int parentsAccount, int teachersAccount, int adminAccount)> GetTotalAccount()
        {
            var studentRole = await _roleManager.FindByNameAsync("Student");
            var studentsCount = await _userManager.GetUsersInRoleAsync(studentRole.Name);

            var parentRole = await _roleManager.FindByNameAsync("Parent");
            var parentsCount = await _userManager.GetUsersInRoleAsync(parentRole.Name);

            var teacherRole = await _roleManager.FindByNameAsync("Teacher");
            var teachersCount = await _userManager.GetUsersInRoleAsync(teacherRole.Name);

            var adminRole = await _roleManager.FindByNameAsync("Admin");
            var adminsCount = await _userManager.GetUsersInRoleAsync(adminRole.Name);

            int totalAccountsCount = studentsCount.Count + parentsCount.Count + teachersCount.Count + adminsCount.Count;
            int studentsAccount = studentsCount.Count;
            int parentsAccount = parentsCount.Count;
            int teachersAccount = teachersCount.Count;
            int adminsAccount = adminsCount.Count;

            return (totalAccountsCount, studentsAccount, parentsAccount, teachersAccount, adminsAccount);
        }

        public async Task<Account> GetByAccountIdAsync(string accountId)
        {
            return await base.GetByStringId(accountId);
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
