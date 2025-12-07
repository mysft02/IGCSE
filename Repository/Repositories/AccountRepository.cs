using BusinessObject;
using BusinessObject.DTOs.Request.Accounts;
using BusinessObject.DTOs.Response.Accounts;
using BusinessObject.Model;
using Common.Utils;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MySqlConnector;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Text;

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

        private (string smtpServer, int smtpPort, string senderEmail, string senderPassword, string senderName) GetEmailSettings()
        {
            var smtpServer = CommonUtils.GetApiKey("EMAIL_SMTP_SERVER");
            var smtpPortStr = CommonUtils.GetApiKey("EMAIL_SMTP_PORT");
            var senderEmail = CommonUtils.GetApiKey("EMAIL_SENDER_EMAIL");
            var senderPassword = CommonUtils.GetApiKey("EMAIL_SENDER_PASSWORD");
            var senderName = CommonUtils.GetApiKey("EMAIL_SENDER_NAME");

            // Kiểm tra và throw exception nếu không tìm thấy trong file
            if (smtpServer.StartsWith("Api Key not found") || smtpServer.StartsWith("Api Key file not found"))
                throw new Exception("EMAIL_SMTP_SERVER không được cấu hình trong file ApiKey.env");
            
            if (smtpPortStr.StartsWith("Api Key not found") || smtpPortStr.StartsWith("Api Key file not found") || !int.TryParse(smtpPortStr, out int smtpPort))
                throw new Exception("EMAIL_SMTP_PORT không được cấu hình trong file ApiKey.env");
            
            if (senderEmail.StartsWith("Api Key not found") || senderEmail.StartsWith("Api Key file not found"))
                throw new Exception("EMAIL_SENDER_EMAIL không được cấu hình trong file ApiKey.env");
            
            if (senderPassword.StartsWith("Api Key not found") || senderPassword.StartsWith("Api Key file not found"))
                throw new Exception("EMAIL_SENDER_PASSWORD không được cấu hình trong file ApiKey.env");
            
            if (senderName.StartsWith("Api Key not found") || senderName.StartsWith("Api Key file not found"))
                senderName = "IGCSE"; // Sender name có thể có default value

            return (smtpServer, smtpPort, senderEmail, senderPassword, senderName);
        }

        public string SendEmail(string recipientEmail, string subject, string htmlBody)
        {
            var (smtpServer, smtpPort, senderEmail, senderPassword, senderName) = GetEmailSettings();

            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(senderEmail, senderPassword);
            smtp.Send(message);
            smtp.Disconnect(true);

            return "Email sent successfully";
        }

        public string SendVerificationEmail(string email, string emailCode)
        {
            var (smtpServer, smtpPort, senderEmail, senderPassword, senderName) = GetEmailSettings();

            StringBuilder emailMessage = new StringBuilder();
            emailMessage.Append("<html>");
            emailMessage.Append("<body>");
            emailMessage.Append($"<p>Dear {email},</p>");
            emailMessage.Append("<p>Thank you for your registering with us. To verify your email address, please use the following verification code: </p>");
            emailMessage.Append($"<h2>Verification Code: {emailCode}</h2>");
            emailMessage.Append("<p>Please enter this code on our website to complete your registration.</p>");
            emailMessage.Append("<p>If you did not request this, please ignore this email</p>");
            emailMessage.Append("<br>");
            emailMessage.Append("<p>Best regards,</p>");
            emailMessage.Append($"<p><strong>{senderName}</strong></p>");
            emailMessage.Append("</body>");
            emailMessage.Append("</html>");

            string message = emailMessage.ToString();

            var _email = new MimeMessage();
            _email.To.Add(MailboxAddress.Parse(email));
            _email.From.Add(new MailboxAddress(senderName, senderEmail));
            _email.Subject = "Email Confirmation";
            _email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

            using var smtp = new SmtpClient();
            smtp.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(senderEmail, senderPassword);
            smtp.Send(_email);
            smtp.Disconnect(true);

            return "Thank you for your registration, kindly check your email for confirmation code";
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
