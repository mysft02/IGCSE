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
using Common.Constants;
using System.Text;
using Service.Request.Accounts;
using Service.Response.Accounts;

namespace Service.Service
{
    public class AccountService
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

            //var userEmail = await GetUser(user.Email);
            //bool isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(userEmail);

            //if (!isEmailConfirmed) 
            //return new BaseResponse<LoginResponse>("You need to confirm email before login", StatusCodeEnum.BadRequest_400, null);

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

            return new BaseResponse<LoginResponse>("Login successfully", StatusCodeEnum.OK_200, loginResponse);
        }

        public async Task<BaseResponse<RegisterResponse>> Register(RegisterRequest request)
        {
            try
            {
                var dateOnly = DateOnly.FromDateTime(request.DateOfBirth);

                var accountApp = new Account
                {
                    UserName = request.Username,
                    Name = request.FullName,
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

                        //string sendEmail = SendEmail(_user!.Email!, emailCode);

                        //if (string.IsNullOrEmpty(sendEmail)) // Nếu gửi thất bại
                        //{
                        //    await _userManager.DeleteAsync(accountApp); // Xóa tài khoản để tránh bị kẹt
                        //    return new BaseResponse<RegisterResponse>("Failed to send email. Please try again.", StatusCodeEnum.InternalServerError_500, null);
                        //}

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

        //public async Task<BaseResponse<string>> Confirmation(string email, int code)
        //{
        //    if (string.IsNullOrEmpty(email) || code <= 0)
        //    {
        //        return new BaseResponse<string>("Invalid Code Provided", StatusCodeEnum.BadRequest_400, null);
        //    }
        //    var user = await GetUser(email);
        //    if (user == null)
        //    {
        //        return new BaseResponse<string>("Invalid Indentity Provided", StatusCodeEnum.BadRequest_400, null);
        //    }
        //    var result = await _userManager.ConfirmEmailAsync(user, code.ToString());
        //    if (!result.Succeeded)
        //    {
        //        return new BaseResponse<string>("Invalid Code Provided", StatusCodeEnum.BadRequest_400, null);
        //    }
        //    else
        //    {
        //        return new BaseResponse<string>("Email confirm successfully, you can proceed to login", StatusCodeEnum.OK_200, null);
        //    }
        //}


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
