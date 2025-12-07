using AutoMapper;
using BusinessObject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.IBaseRepository;
using Repository.IRepositories;
using Common.Constants;
using BusinessObject.DTOs.Request.ParentStudentLink;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Accounts;
using BusinessObject.DTOs.Request.Accounts;
using BusinessObject.DTOs.Response;

namespace Service
{
    public class AccountService
    {
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly SignInManager<Account> _signinManager;
        private readonly IParentStudentLinkRepository _parentStudentLinkRepository;

        public AccountService(IMapper mapper,
            IAccountRepository accountRepository,
            UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenRepository tokenRepository,
            SignInManager<Account> signinManager,
            IParentStudentLinkRepository parentStudentLinkRepository)
        {
            _mapper = mapper;
            _accountRepository = accountRepository;
            _parentStudentLinkRepository = parentStudentLinkRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenRepository = tokenRepository;
            _signinManager = signinManager;
        }

        public async Task<BaseResponse<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.Username.ToLower());

            if (user == null)
            {
                throw new Exception("tên người dùng không hợp lệ");
            }

            if (user.Status == false)
            {
              throw new Exception("Không đăng nhập bằng tài khoản này được nữa");
            }

            // Kiểm tra role của user
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin");

            // Kiểm tra email đã được xác thực chưa (bỏ qua nếu là Admin)
            if (!isAdmin && !await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new Exception("Email chưa được xác thực. Vui lòng kiểm tra email và xác thực tài khoản trước khi đăng nhập.");
            }

            var result = await _signinManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                throw new Exception("Mật khẩu không chính xác");
            }

            var token = await _tokenRepository.createToken(user);
            var loginResponse = new LoginResponse
            {
                UserID = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = (roles?.FirstOrDefault() is string singleRoleLogin && !string.IsNullOrWhiteSpace(singleRoleLogin))
                    ? new List<string> { singleRoleLogin }
                    : new List<string>(),
                Phone = user.Phone,
                isActive = user.Status,
                Name = user.Name,
                Address = user.Address,
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken
            };

            return new BaseResponse<LoginResponse>("Đăng nhập thành công", StatusCodeEnum.OK_200, loginResponse);
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
                    throw new Exception("Email đã tồn tại");
                }

                // Kiểm tra số điện thoại đã tồn tại chưa
                var existUserByPhone = await _accountRepository.FindOneAsync(x => x.Phone == request.Phone);
                if (existUserByPhone != null)
                {
                    throw new Exception("Số điện thoại đã tồn tại");
                }

                // Ensure Student role exists
                if (!await _roleManager.RoleExistsAsync("Student"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("Student"));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Tạo vai trò học sinh thất bại: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }

                var createdUser = await _userManager.CreateAsync(accountApp, request.Password);
                if (createdUser.Succeeded)
                {
                    // Automatically assign Student role to new users
                    var roleResult = await _userManager.AddToRoleAsync(accountApp, "Student");
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(accountApp); // Clean up the user if role assignment fails
                        throw new Exception($"Gán vai trò học sinh thất bại: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }

                    // Generate email confirmation token
                    var _user = await GetUser(request.Email);
                    var emailCode = await _userManager.GenerateEmailConfirmationTokenAsync(_user!);

                    if (string.IsNullOrEmpty(emailCode))
                    {
                        await _userManager.DeleteAsync(accountApp); // Xóa tài khoản để tránh bị kẹt
                        throw new Exception("Failed to generate confirmation token. Please try again.");
                    }

                    // Send verification email
                    try
                    {
                        _accountRepository.SendVerificationEmail(request.Email, emailCode);
                    }
                    catch (Exception ex)
                    {
                        await _userManager.DeleteAsync(accountApp); // Xóa tài khoản nếu không gửi được email
                        throw new Exception($"Không thể gửi email xác thực: {ex.Message}");
                    }

                    // Không trả về token, yêu cầu user verify email trước
                    var customerResponse = new RegisterResponse
                    {
                        UserName = accountApp.UserName,
                        Email = accountApp.Email,
                        Name = accountApp.Name,
                        Address = accountApp.Address,
                        Phone = accountApp.Phone,
                        DateOfBirth = accountApp.DateOfBirth,
                        Roles = new List<string>(),
                        Token = null,
                        RefreshToken = null
                    };
                    return new BaseResponse<RegisterResponse>("Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.", StatusCodeEnum.OK_200, customerResponse);
                }
                else
                {
                    throw new Exception($"{createdUser.Errors}");
                }
            }
            catch (Exception e)
            {
                var errorMessage = e.Message;

                // Nếu có lỗi bên trong, gộp lại
                if (e.InnerException != null)
                {
                    errorMessage += " | InnerException: " + e.InnerException.Message;

                    if (e.InnerException.InnerException != null)
                    {
                        errorMessage += " | InnerInnerException: " + e.InnerException.InnerException.Message;
                    }
                }
                throw new Exception($"{errorMessage}");
            }
        }

        private async Task<Account> GetUser(string email)
        => await _userManager.FindByEmailAsync(email);
        
        public async Task<BaseResponse<AccountChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            var user = await _userManager.FindByNameAsync(changePassword.UserName);
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại");
            }
            if (string.Compare(changePassword.NewPassword, changePassword.ConfirmNewPassword) != 0)
            {
                throw new Exception("Mật khẩu và xác nhận mật khẩu không chính xác");
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
                throw new Exception(errorMessage);
            }

            var response = new AccountChangePasswordResponse
            {
                Username = changePassword.UserName,
                Password = changePassword.NewPassword,
                ConfirmPassword = changePassword.ConfirmNewPassword
            };
            return new BaseResponse<AccountChangePasswordResponse>("Đổi mật khẩu thành công", StatusCodeEnum.OK_200, response);
        }

        public async Task<BaseResponse<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Không tiết lộ thông tin email có tồn tại hay không vì lý do bảo mật
                throw new Exception("Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn");
            }

            if (user.Status == false)
            {
                throw new Exception("Tài khoản này đã bị khóa");
            }

            // Tạo token reset password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Tạo URL reset password (có thể tùy chỉnh theo frontend URL)
            var resetUrl = $"https://your-frontend-url.com/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(resetToken)}";
            
            // Tạo nội dung email
            var emailSubject = "Đặt lại mật khẩu";
            var emailBody = $@"
                <html>
                <body>
                    <h2>Yêu cầu đặt lại mật khẩu</h2>
                    <p>Xin chào {user.Name},</p>
                    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                    <p>Vui lòng click vào link sau để đặt lại mật khẩu:</p>
                    <p><a href=""{resetUrl}"" style=""background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Đặt lại mật khẩu</a></p>
                    <p>Hoặc copy link sau vào trình duyệt:</p>
                    <p>{resetUrl}</p>
                    <p>Link này sẽ hết hạn sau 1 giờ.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>Đội ngũ IGCSE</p>
                </body>
                </html>";

            // Gửi email
            try
            {
                _accountRepository.SendEmail(request.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể gửi email: {ex.Message}");
            }

            var response = new ForgotPasswordResponse
            {
                Email = request.Email,
                Message = "Chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn. Vui lòng kiểm tra hộp thư."
            };

            return new BaseResponse<ForgotPasswordResponse>("Gửi email thành công", StatusCodeEnum.OK_200, response);
        }

        public async Task<BaseResponse<string>> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("Email không tồn tại");
            }

            if (user.Status == false)
            {
                throw new Exception("Tài khoản này đã bị khóa");
            }

            // Xác thực token và reset password
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                string errorMessage = string.Join(" | ", errors);
                throw new Exception($"Đặt lại mật khẩu thất bại: {errorMessage}");
            }

            return new BaseResponse<string>("Đặt lại mật khẩu thành công", StatusCodeEnum.OK_200, "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập lại.");
        }

        public async Task<BaseResponse<VerifyEmailResponse>> VerifyEmail(VerifyEmailRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("Email không tồn tại");
            }

            if (user.Status == false)
            {
                throw new Exception("Tài khoản này đã bị khóa");
            }

            // Kiểm tra email đã được xác thực chưa
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new Exception("Email đã được xác thực trước đó");
            }

            // Xác thực email với token
            var result = await _userManager.ConfirmEmailAsync(user, request.VerificationCode);
            
            if (!result.Succeeded)
            {
                var errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                string errorMessage = string.Join(" | ", errors);
                throw new Exception($"Xác thực email thất bại: {errorMessage}");
            }

            // Sau khi xác thực thành công, tạo token để user có thể đăng nhập ngay
            var token = await _tokenRepository.createToken(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var response = new VerifyEmailResponse
            {
                Email = request.Email,
                Message = "Email đã được xác thực thành công. Bạn có thể đăng nhập ngay.",
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken
            };

            return new BaseResponse<VerifyEmailResponse>("Xác thực email thành công", StatusCodeEnum.OK_200, response);
        }

        public async Task<Account> GetByStringId(string id)
        {
            var account = await _accountRepository.GetByStringId(id);
            if (account == null)
            {
                throw new ArgumentException("Không tìm thấy tài khoản");
            }
            return account;
        }

        public async Task<NewUserDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
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
                Roles = (roles?.FirstOrDefault() is string singleRoleProf && !string.IsNullOrWhiteSpace(singleRoleProf))
                    ? new List<string> { singleRoleProf }
                    : new List<string>()
            };

            return userProfile;
        }

        public async Task<BaseResponse<SetRoleResponse>> SetUserRoleAsync(string currentUserId, SetRoleRequest request)
        {
            try
            {
                // Kiểm tra người dùng hiện tại có được xác thực không
                if (string.IsNullOrEmpty(currentUserId))
                {
                    throw new Exception("Người dùng chưa đăng nhập");
                }

                // Kiểm tra quyền Admin
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    throw new Exception("Không tìm thấy thông tin người dùng hiện tại");
                }

                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                if (!currentUserRoles.Contains("Admin"))
                {
                    throw new Exception("Chỉ Admin mới có quyền thay đổi role của người dùng");
                }

                // Tìm user cần thay đổi role
                var targetUser = await _userManager.FindByIdAsync(request.UserId);
                if (targetUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng cần thay đổi role");
                }

                // Lấy role hiện tại
                var currentRoles = await _userManager.GetRolesAsync(targetUser);
                var oldRole = currentRoles.FirstOrDefault() ?? "Student";

                // Xóa tất cả role hiện tại
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
                }

                // Thêm role mới
                var result = await _userManager.AddToRoleAsync(targetUser, request.Role);
                if (!result.Succeeded)
                {
                    throw new Exception($"Không thể thay đổi role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Nếu role mới là Admin, tự động xác thực email
                if (request.Role == "Admin" && !await _userManager.IsEmailConfirmedAsync(targetUser))
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(targetUser);
                    await _userManager.ConfirmEmailAsync(targetUser, emailConfirmationToken);
                }

                // Tạo response
                var response = new SetRoleResponse
                {
                    UserId = targetUser.Id,
                    UserName = targetUser.UserName,
                    Email = targetUser.Email,
                    Name = targetUser.Name,
                    OldRole = oldRole,
                    NewRole = request.Role,
                    UpdatedAt = DateTime.UtcNow
                };

                return new BaseResponse<SetRoleResponse>(
                    $"Đã thay đổi role của {targetUser.Name} từ {oldRole} thành {request.Role}",
                    StatusCodeEnum.OK_200,
                    response
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Thay đổi vai trò thất bại: {ex.Message}");
            }
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
                    Roles = (roles?.FirstOrDefault() is string singleRoleList && !string.IsNullOrWhiteSpace(singleRoleList))
                        ? new List<string> { singleRoleList }
                        : new List<string>()
                });
            }
            return accountInfoList;
        }

        public async Task<BaseResponse<PaginatedResponse<NewUserDto>>> GetAccountsPagedAsync(AccountListQuery query)
        {
            var result = await _accountRepository.GetPagedUserList(query);

            var items = new List<NewUserDto>();
            foreach (var user in result.items)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!string.IsNullOrWhiteSpace(query.Role?.ToString()))
                {
                    if (!roles.Contains(query.Role.ToString()))
                    {
                        continue;
                    }
                }
                items.Add(new NewUserDto
                {
                    UserID = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Address = user.Address,
                    Phone = user.Phone,
                    isActive = user.Status,
                    Roles = (roles?.FirstOrDefault() is string singleRoleList && !string.IsNullOrWhiteSpace(singleRoleList))
                        ? new List<string> { singleRoleList }
                        : new List<string>()
                });
            }

            var paginated = new PaginatedResponse<NewUserDto>
            {
                Items = items,
                TotalCount = result.totalCount,
                Page = result.page - 1,
                Size = result.size,
                TotalPages = (int)Math.Ceiling(result.totalCount / (double)query.PageSize)
            };

            return new BaseResponse<PaginatedResponse<NewUserDto>>(
                "Lấy danh sách tài khoản thành công",
                StatusCodeEnum.OK_200,
                paginated
            );
        }

        public async Task<BaseResponse<ParentStudentLinkResponse>> LinkStudentToParentAsync(ParentStudentLinkRequest request)
        {
            var parent = await _accountRepository.GetByStringId(request.ParentId);
            var student = await _accountRepository.FindOneAsync(x => x.Email == request.StudentEmail);

            if (student == null)
            {
                throw new Exception("Không tìm thấy tài khoản học sinh này.");
            }

            var studentRole = await _userManager.GetRolesAsync(student);
            if (!studentRole.Contains("Student"))
            {
                throw new Exception("Tài khoản này không phải học sinh.");
            }

            var checkDuplicate = await _parentStudentLinkRepository.FindOneAsync(x => x.ParentId == request.ParentId && x.StudentId == student.Id);
            if(checkDuplicate != null)
            {
                throw new Exception("Bạn đã liên kết tài khoản học sinh này rồi");
            }

            var parentStudentLink = new Parentstudentlink
            {
                ParentId = request.ParentId,
                StudentId = student.Id,
            };

            var result = await _parentStudentLinkRepository.AddAsync(parentStudentLink);
            var response = _mapper.Map<ParentStudentLinkResponse>(result);

            return new BaseResponse<ParentStudentLinkResponse>(
                "Liên kết tài khoản học sinh thành công",
                StatusCodeEnum.Created_201,
                response
            );
        }

        public async Task<BaseResponse<IEnumerable<AccountResponse>>> GetAllStudentsByParentId(string parentId)
        {
            var parent = await _accountRepository.GetByStringId(parentId);

            if (parent == null)
            {
                throw new Exception("Không tìm thấy phụ huynh này");
            }

            var parentRole = await _userManager.GetRolesAsync(parent);

            if (!parentRole.Contains("Parent"))
            {
                throw new Exception("Bạn không phải là phụ huynh");
            }

            var result = await _parentStudentLinkRepository.GetByParentId(parentId);
            var response = _mapper.Map<IEnumerable<AccountResponse>>(result);

            return new BaseResponse<IEnumerable<AccountResponse>>(
                "Liên kết phụ huynh - học sinh thành công",
                StatusCodeEnum.Created_201,
                response
            );
        }
    }
}
