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
                throw new Exception("Invalid username!");
            }

            if (user.Status == false)
            {
              throw new Exception("Cannot login with this account anymore");
            }


            var result = await _signinManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                throw new Exception("Username not found and/or password incorrect");
            }

            var roles = await _userManager.GetRolesAsync(user);

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
                    throw new Exception("Email already exists!");
                }
                else
                {
                    // Ensure Student role exists
                    if (!await _roleManager.RoleExistsAsync("Student"))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole("Student"));
                        if (!roleResult.Succeeded)
                        {
                            throw new Exception($"Failed to create Student role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
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
                            throw new Exception($"Failed to assign Student role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }

                        var token = await _tokenRepository.createToken(accountApp);
                        var _user = await GetUser(request.Email);
                        var emailCode = await _userManager.GenerateEmailConfirmationTokenAsync(_user!);

                        if (string.IsNullOrEmpty(emailCode))
                        {
                            await _userManager.DeleteAsync(accountApp); // Xóa tài khoản để tránh bị kẹt
                            throw new Exception("Failed to generate confirmation token. Please try again.");
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
                            Roles = (userRoles?.FirstOrDefault() is string singleRoleReg && !string.IsNullOrWhiteSpace(singleRoleReg))
                                ? new List<string> { singleRoleReg }
                                : new List<string>(),
                            Token = token.AccessToken,
                            RefreshToken = token.RefreshToken
                        };
                        return new BaseResponse<RegisterResponse>("Register successfully", StatusCodeEnum.OK_200, customerResponse);
                    }
                    else
                    {
                        throw new Exception($"{createdUser.Errors}");
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
                throw new Exception("User Not Exist");
            }
            if (string.Compare(changePassword.NewPassword, changePassword.ConfirmNewPassword) != 0)
            {
                throw new Exception("Password and ConfirmPassword doesnot match! ");
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
                throw new Exception($"Failed to set user role: {ex.Message}");
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
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var usersQuery = _userManager.Users.AsQueryable();

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

            var items = new List<NewUserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!string.IsNullOrWhiteSpace(query.Role))
                {
                    if (!roles.Contains(query.Role))
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
                TotalCount = total,
                Page = page - 1,
                Size = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return new BaseResponse<PaginatedResponse<NewUserDto>>(
                "Accounts retrieved successfully",
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
                throw new Exception("Parent not found.");
            }

            var parentRole = await _userManager.GetRolesAsync(parent);

            if (!parentRole.Contains("Parent"))
            {
                throw new Exception("You are not a parent.");
            }

            var result = await _parentStudentLinkRepository.GetByParentId(parentId);
            var response = _mapper.Map<IEnumerable<AccountResponse>>(result);

            return new BaseResponse<IEnumerable<AccountResponse>>(
                "Parent-Student link created successfully",
                StatusCodeEnum.Created_201,
                response
            );
        }
    }
}
