using DTOs.Request.Accounts;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service;
using BusinessObject.DTOs.Request.ParentStudentLink;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.Model;

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
                // Lấy user hiện tại từ claims trong JWT token
                var user = HttpContext.User;
                var currentUserId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new BaseResponse<string>(
                        "Không tìm thấy thông tin người dùng hiện tại",
                        Common.Constants.StatusCodeEnum.Unauthorized_401,
                        null
                    ));
                }

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

        [HttpPost("link-student-to-parent")]
        public async Task<ActionResult<BaseResponse<ParentStudentLinkResponse>>> LinkStudentToParent([FromBody] ParentStudentLinkRequest request)
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

            var result = await _accountService.LinkStudentToParentAsync(request);
            return Ok(result);
        }

        [HttpGet("get-all-students-belong-to-parent")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AccountResponse>>>> GetListStudentsBelongToParent([FromQuery] string parentId)
        {
            var result = await _accountService.GetAllStudentsByParentId(parentId);
            return Ok(result);
        }
    }
}
