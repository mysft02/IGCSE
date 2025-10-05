using DTOs.Request.Accounts;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("change-password")]
        public async Task<BaseResponse<AccountChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            return await _accountService.ChangePassword(changePassword);
        }
    }
}
