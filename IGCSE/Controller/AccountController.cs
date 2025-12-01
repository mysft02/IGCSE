using Microsoft.AspNetCore.Mvc;
using Service;
using BusinessObject.DTOs.Request.ParentStudentLink;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Accounts;
using BusinessObject.DTOs.Request.Accounts;
using Microsoft.AspNetCore.Authorization;
using BusinessObject.DTOs.Response.ParentStudentLink;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpGet("get-all-account")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Lấy danh sách tài khoản",
            Description = @"Api trả về danh sách tài khoản cho admin quản lí:

**Request `AccountListQuery`:**
- `SearchByName` (string): tìm kiếm theo tên tài khoản

- `Role` (`UserRoleEnum`): tìm kiếm theo role của tài khoản 
**Lưu ý**: `1` là `Admin`, `2` là `Teacher`, `3` là `Parent`, `4` là `Student`, `5` là `Manager`

- `IsActive` (bool): tìm kiếm theo trang thái hoạt động tài khoản. `true` là hoạt động, `false` là ngừng hoạt động

**Response `FinalQuizResultReviewResponse`:**
- Schema :
```json
{
  ""message"": ""string"",
  ""statusCode"": 100,
  ""data"": {
    ""items"": [
      {
        ""userID"": ""string"",
        ""userName"": ""string"",
        ""email"": ""string"",
        ""name"": ""string"",
        ""address"": ""string"",
        ""phone"": ""string"",
        ""isActive"": true,
        ""roles"": [
          ""string""
        ]
      }
    ],
    ""totalCount"": 0,
    ""page"": 0,
    ""size"": 0,
    ""totalPages"": 0,
    ""hasNextPage"": true,
    ""hasPreviousPage"": true,
    ""currentPage"": 0
  }
}
```
")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<NewUserDto>>>> GetAllAccountsAsync([FromQuery] AccountListQuery query)
        {
            var result = await _accountService.GetAccountsPagedAsync(query);
            return Ok(result);
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
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(Summary = "Liên kết tài khoản học sinh với phụ huynh", Description = "Truyền vào `email` của tài khoản học sinh cần liên kết khi cần liên kết tài khoản học sinh với phụ huynh, kết quả trả về sẽ là `id` của `parent` và `student`")]
        public async Task<ActionResult<BaseResponse<ParentStudentLinkResponse>>> LinkStudentToParent([FromBody] string studentEmail)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

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

            var request = new ParentStudentLinkRequest
            {
                ParentId = userId,
                StudentEmail = studentEmail
            };

            var result = await _accountService.LinkStudentToParentAsync(request);
            return Ok(result);
        }

        [HttpGet("get-all-students-belong-to-parent")]
        [Authorize(Roles = "Parent")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AccountResponse>>>> GetListStudentsBelongToParent()
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _accountService.GetAllStudentsByParentId(userId);
            return Ok(result);
        }
    }
}
