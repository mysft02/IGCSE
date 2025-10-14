using DTOs.Request.Accounts;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service;

namespace IGCSE.Controller
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var userProfile = await _accountService.GetProfileAsync(userId);
            if (userProfile == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            return Ok(userProfile);
        }

        [HttpGet]
        [Route("get-all-account")]
        public async Task<List<NewUserDto>> GetAllAccountsAsync()
        {
            return await _accountService.GetAllAccountsAsync();
        }

        [HttpPost("login")]
        public async Task<BaseResponse<LoginResponse>> Login(LoginRequest request)
        {
            return await _accountService.Login(request);
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<RegisterResponse>>> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { Message = "Dữ liệu không hợp lệ", Errors = errors });
            }
            return await _accountService.Register(request);
        }

        [HttpPost("set-role")]
        public async Task<ActionResult<BaseResponse<SetRoleResponse>>> SetUserRole([FromBody] SetRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                // COMMENTED FOR TESTING PAYMENT FLOW - Lấy user hiện tại từ claims
                // var user = HttpContext.User;
                // var currentUserId = user.FindFirst("AccountID")?.Value;
                
                // if (string.IsNullOrEmpty(currentUserId))
                // {
                //     return Unauthorized(new BaseResponse<string>(
                //         "Không tìm thấy thông tin người dùng",
                //         Common.Constants.StatusCodeEnum.Unauthorized_401,
                //         null
                //     ));
                // }

                // FOR TESTING: Sử dụng một userId cố định
                var currentUserId = "test-user-id";

                var result = await _accountService.SetUserRoleAsync(currentUserId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("change-password")]
        public async Task<BaseResponse<AccountChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            return await _accountService.ChangePassword(changePassword);
        }

        [HttpGet("debug-user-info")]
        // [Authorize] // COMMENTED FOR TESTING PAYMENT FLOW
        public async Task<ActionResult<BaseResponse<object>>> DebugUserInfo()
        {
            try
            {
                // COMMENTED FOR TESTING PAYMENT FLOW - Authentication logic
                // var user = HttpContext.User;
                // var userId = user.FindFirst("AccountID")?.Value;
                // var roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList();
                // var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                // var username = user.FindFirst("given_name")?.Value;

                // FOR TESTING: Sử dụng dữ liệu giả lập
                var debugInfo = new
                {
                    UserId = "test-user-id",
                    Email = "test@example.com",
                    Username = "Test User",
                    Roles = new List<string> { "Parent" },
                    AllClaims = new List<object> { new { Type = "AccountID", Value = "test-user-id" } },
                    IsAuthenticated = true,
                    AuthenticationType = "JWT"
                };

                return Ok(new BaseResponse<object>(
                    "Thông tin debug user (TEST MODE)",
                    Common.Constants.StatusCodeEnum.OK_200,
                    debugInfo
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("create-parent-account")]
        public async Task<ActionResult<BaseResponse<object>>> CreateParentAccount()
        {
            try
            {
                var result = await _accountService.CreateParentAccountAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}
