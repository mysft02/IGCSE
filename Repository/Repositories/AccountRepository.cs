using BusinessObject;
using BusinessObject.DTOs.Request.Accounts;
using BusinessObject.DTOs.Response.Accounts;
using BusinessObject.Model;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MySqlConnector;
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

        public async Task<(List<Account> items, int totalCount, int page, int size)> GetPagedUserList(AccountListQuery query)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Role.ToString()))
            {
                var roleNameParam = new MySqlParameter("@RoleName", query.Role.ToString());

                usersQuery = _context.Users
                    .FromSqlRaw(@"SELECT u.*
            FROM AspNetUsers u
            JOIN AspNetUserRoles ur ON u.Id = ur.UserId
            JOIN AspNetRoles r ON ur.RoleId = r.Id
            WHERE r.Name = @RoleName", roleNameParam)
                    .AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(query.SearchByName))
            {
                var keyword = query.SearchByName.Trim();
                usersQuery = usersQuery.Where(u => u.UserName.Contains(keyword) || u.Name.Contains(keyword));
            }

            if (query.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.Status == query.IsActive.Value);
            }

            var total = await usersQuery.CountAsync();
            var skip = (page <= 1 ? 0 : (page - 1) * pageSize);
            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new (users, total, page, pageSize);
        }
    }
}
