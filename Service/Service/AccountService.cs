using AutoMapper;
using BusinessObject.IdentityModel;
using BusinessObject.Model;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Repository.IBaseRepository;
using Repository.IRepositories;
using Service.IService;
using Service.RequestAndResponse.BaseResponse;
using Service.RequestAndResponse.Enums;
using Service.RequestAndResponse.Request.Accounts;
using Service.RequestAndResponse.Response.Accounts;
using System.Text;

namespace Service.Service
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly SignInManager<Account> _signinManager;

        public AccountService(IMapper mapper,
            IAccountRepository accountRepository,
            UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenRepository tokenRepository,
            SignInManager<Account> signinManager)
        {
            _mapper = mapper;
            _accountRepository = accountRepository;
            
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenRepository = tokenRepository;
            _signinManager = signinManager;
        }

        public async Task<BaseResponse<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.Username.ToLower());

            if (user == null) 
            return new BaseResponse<LoginResponse>("Invalid username!", StatusCodeEnum.Unauthorized_401, null);

            var userEmail = await GetUser(user.Email);
            bool isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(userEmail);

            if (!isEmailConfirmed) 
            return new BaseResponse<LoginResponse>("You need to confirm email before login", StatusCodeEnum.BadRequest_400, null);

            if (user.Status == false)
            {
                return new BaseResponse<LoginResponse>("Cannot login with this account anymore!", StatusCodeEnum.Unauthorized_401, null);
            }


            var result = await _signinManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded) 
            return new BaseResponse<LoginResponse>("Username not found and/or password incorrect", StatusCodeEnum.Unauthorized_401, null);

            var roles = await _userManager.GetRolesAsync(user);

            var token = await _tokenRepository.createToken(user);
            var loginResponse = new LoginResponse
            {
                UserID = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                Phone = user.Phone,
                isActive = user.Status,
                Name = user.Name,
                Address = user.Address,
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken
            };

            return new BaseResponse<LoginResponse>("Register successfully", StatusCodeEnum.OK_200, loginResponse);
        }

        public async Task<BaseResponse<RegisterResponse>> Register(RegisterRequest request)
        {
            try
            {
                var dateOnly = DateOnly.FromDateTime(request.DateOfBirth);

                var accountApp = new Account
                {
                    UserName = request.Username,
                    Name = request.Name,
                    Email = request.Email,
                    Address = request.Address,
                    Phone = request.Phone,
                    Status = true,
                    DateOfBirth = dateOnly
                };
                var existUser = await _userManager.FindByEmailAsync(request.Email);
                if (existUser != null)
                {
                    return new BaseResponse<RegisterResponse>("Email already exists!", StatusCodeEnum.Conflict_409, null);
                }
                else
                {
                    var createdUser = await _userManager.CreateAsync(accountApp, request.Password);
                    if (createdUser.Succeeded)
                    {
                        var token = await _tokenRepository.createToken(accountApp);
                        var _user = await GetUser(request.Email);
                        var emailCode = await _userManager.GenerateEmailConfirmationTokenAsync(_user!);

                        if (string.IsNullOrEmpty(emailCode))
                        {
                            await _userManager.DeleteAsync(accountApp); // Xóa tài khoản để tránh bị kẹt
                            return new BaseResponse<RegisterResponse>("Failed to generate confirmation token. Please try again.", StatusCodeEnum.InternalServerError_500, null);
                        }

                        string sendEmail = SendEmail(_user!.Email!, emailCode);

                        if (string.IsNullOrEmpty(sendEmail)) // Nếu gửi thất bại
                        {
                            await _userManager.DeleteAsync(accountApp); // Xóa tài khoản để tránh bị kẹt
                            return new BaseResponse<RegisterResponse>("Failed to send email. Please try again.", StatusCodeEnum.InternalServerError_500, null);
                        }

                        var userRoles = await _userManager.GetRolesAsync(accountApp);
                        var customerResponse = new RegisterResponse
                        {
                            UserName = accountApp.UserName,
                            Email = accountApp.Email,
                            Name = accountApp.Name,
                            Address = accountApp.Address,
                            Phone = accountApp.Phone,
                            DateOfBirth = accountApp.DateOfBirth,
                            Roles = userRoles.ToList(), // This will be an empty list now
                            Token = token.AccessToken,
                            RefreshToken = token.RefreshToken
                        };
                        return new BaseResponse<RegisterResponse>("Register successfully", StatusCodeEnum.OK_200, customerResponse);
                    }
                    else
                    {
                        return new BaseResponse<RegisterResponse>($"{createdUser.Errors}", StatusCodeEnum.InternalServerError_500, null);
                    }
                }
            }
            catch (Exception e)
            {
                var errorMessage = e.Message;

                // Nếu có lỗi bên trong, gộp lại
                if (e.InnerException != null)
                {
                    errorMessage += " | InnerException: " + e.InnerException.Message;

                    // Nếu còn lồng nữa (2 cấp), bạn có thể lấy tiếp:
                    if (e.InnerException.InnerException != null)
                    {
                        errorMessage += " | InnerInnerException: " + e.InnerException.InnerException.Message;
                    }
                }
                return new BaseResponse<RegisterResponse>($"{errorMessage}", StatusCodeEnum.InternalServerError_500, null);
            }
        }

        private async Task<Account> GetUser(string email)
        => await _userManager.FindByEmailAsync(email);

        private string SendEmail(string email, string emailCode)
        {
            StringBuilder emailMessage = new StringBuilder();
            emailMessage.Append("<html>");
            emailMessage.Append("<body>");
            emailMessage.Append($"<p>Dear {email},</p>");
            emailMessage.Append("<p>Thank you for your registering with us. To verifiy your email address, please use the following verification code: </p>");
            emailMessage.Append($"<h2>Verification Code: {emailCode}</h2>");
            emailMessage.Append("<p>Please enter this code on our website to complete your registration.</p>");
            emailMessage.Append("<p>If you did not request this, please ignore this email</p>");
            emailMessage.Append("<br>");
            emailMessage.Append("<p>Best regards,</p>");
            emailMessage.Append("<p><strong>GHSMSystem</strong></o>");
            emailMessage.Append("</body>");
            emailMessage.Append("</html>");

            string message = emailMessage.ToString();
            var _email = new MimeMessage();
            _email.To.Add(MailboxAddress.Parse(email));
            _email.From.Add(MailboxAddress.Parse("huydqse173363@fpt.edu.vn"));
            _email.Subject = "Email Confirmation";
            _email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("huydqse173363@fpt.edu.vn", "hmkf wdjs epwx ibfg");
            smtp.Send(_email);
            smtp.Disconnect(true);
            return "Thank you for your registration, kindly check your email for confirmation code";
        }

        public async Task<BaseResponse<string>> Confirmation(string email, int code)
        {
            if (string.IsNullOrEmpty(email) || code <= 0)
            {
                return new BaseResponse<string>("Invalid Code Provided", StatusCodeEnum.BadRequest_400, null);
            }
            var user = await GetUser(email);
            if (user == null)
            {
                return new BaseResponse<string>("Invalid Indentity Provided", StatusCodeEnum.BadRequest_400, null);
            }
            var result = await _userManager.ConfirmEmailAsync(user, code.ToString());
            if (!result.Succeeded)
            {
                return new BaseResponse<string>("Invalid Code Provided", StatusCodeEnum.BadRequest_400, null);
            }
            else
            {
                return new BaseResponse<string>("Email confirm successfully, you can proceed to login", StatusCodeEnum.OK_200, null);
            }
        }

        public async Task<BaseResponse<ApiResponse>> RenewToken(TokenModel model)
        {
            var result = await _tokenRepository.renewToken(model);
            return new BaseResponse<ApiResponse>("Renew Token Successfully", StatusCodeEnum.OK_200, result);
        }



        public async Task<BaseResponse<AccountChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            var user = await _userManager.FindByNameAsync(changePassword.UserName);
            if (user == null)
            {
                return new BaseResponse<AccountChangePasswordResponse>("User Not Exist", StatusCodeEnum.BadRequest_400, null);
            }
            if (string.Compare(changePassword.NewPassword, changePassword.ConfirmNewPassword) != 0)
            {
                return new BaseResponse<AccountChangePasswordResponse>("Password and ConfirmPassword doesnot match! ", StatusCodeEnum.BadRequest_400, null);
            }
            var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (!result.Succeeded)
            {
                var errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                // Join the errors into a single string
                string errorMessage = string.Join(" | ", errors);
                return new BaseResponse<AccountChangePasswordResponse>(errorMessage, StatusCodeEnum.BadRequest_400, null);
            }

            var response = new AccountChangePasswordResponse
            {
                Username = changePassword.UserName,
                Password = changePassword.NewPassword,
                ConfirmPassword = changePassword.ConfirmNewPassword
            };
            return new BaseResponse<AccountChangePasswordResponse>("Password changed successfully.", StatusCodeEnum.OK_200, response);
        }

        public async Task<Account> GetByStringId(string id)
        {
            var account = await _accountRepository.GetByStringId(id);
            if (account == null)
            {
                throw new ArgumentException("Cannot Find account!");
            }
            return account;
        }

        public async Task<NewUserDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null; // Or throw an exception, depending on desired behavior
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userProfile = new NewUserDto
            {
                UserID = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Address = user.Address,
                Phone = user.Phone,
                isActive = user.Status,
                Roles = roles.ToList()
            };

            return userProfile;
        }

        public async Task<List<NewUserDto>> GetAllAccountsAsync()
        {
            var allAccounts = await _userManager.Users.ToListAsync();

            var accountInfoList = new List<NewUserDto>();

            foreach (var user in allAccounts)
            {
                var roles = await _userManager.GetRolesAsync(user);

                accountInfoList.Add(new NewUserDto
                {
                    UserID = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Address = user.Address,
                    Phone = user.Phone,
                    isActive = user.Status,
                    Roles = roles.ToList()
                });
            }

            return accountInfoList;
        }
    }
}
