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
        [SwaggerOperation(
            Summary = "Lấy thông tin profile của user", 
            Description = @"Api dùng để lấy thông tin chi tiết profile của một user theo userId.

**Request:**
- Path parameter: `userId` (string) - ID của user cần lấy thông tin

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Profile retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""userID"": ""user-id-123"",
    ""userName"": ""username"",
    ""email"": ""user@example.com"",
    ""name"": ""Nguyễn Văn A"",
    ""address"": ""123 Đường ABC"",
    ""phone"": ""0123456789"",
    ""dateOfBirth"": ""2000-01-01"",
    ""isActive"": true,
    ""roles"": [""Student""]
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **User không tồn tại:**
```json
{
  ""message"": ""User not found"",
  ""statusCode"": 404
}")]
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
        [SwaggerOperation(
            Summary = "Lấy danh sách tất cả tài khoản (có paging và filter)", 
            Description = @"Api dùng để lấy danh sách tất cả tài khoản trong hệ thống với phân trang và bộ lọc.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 1) - Số trang
  - `PageSize` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `AccountListQuery`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Accounts retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""userID"": ""user-id-123"",
        ""userName"": ""username"",
        ""email"": ""user@example.com"",
        ""name"": ""Nguyễn Văn A"",
        ""isActive"": true,
        ""roles"": [""Student""]
      }
    ],
    ""totalCount"": 100,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 10
  }
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Error message"",
  ""statusCode"": 400,
  ""data"": null
}")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<NewUserDto>>>> GetAllAccountsAsync([FromQuery] AccountListQuery query)
        {
            var result = await _accountService.GetAccountsPagedAsync(query);
            return Ok(result);
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Đăng nhập vào hệ thống", 
            Description = @"Api dùng để user đăng nhập vào hệ thống bằng username và password. Sau khi đăng nhập thành công, hệ thống trả về JWT token và refresh token.

**Request:**
- Body:
```json
{
  ""username"": ""username"",
  ""password"": ""password123""
}
```
  - `username` (string, required) - Tên đăng nhập
  - `password` (string, required) - Mật khẩu

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Login successful"",
  ""statusCode"": 200,
  ""data"": {
    ""userID"": ""user-id-123"",
    ""userName"": ""username"",
    ""email"": ""user@example.com"",
    ""name"": ""Nguyễn Văn A"",
    ""address"": ""123 Đường ABC"",
    ""phone"": ""0123456789"",
    ""dateOfBirth"": ""2000-01-01"",
    ""isActive"": true,
    ""roles"": [""Student""],
    ""token"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."",
    ""refreshToken"": ""refresh-token-here""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Username hoặc password không đúng:**
```json
{
  ""message"": ""Invalid username or password"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Tài khoản bị khóa:**
```json
{
  ""message"": ""Account is locked"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Username and password are required"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Token có thời hạn sử dụng, cần sử dụng refresh token để lấy token mới
- Refresh token dùng để làm mới access token khi hết hạn")]
        public async Task<BaseResponse<LoginResponse>> Login(LoginRequest request)
        {
            return await _accountService.Login(request);
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Đăng ký tài khoản mới", 
            Description = @"Api dùng để đăng ký tài khoản mới trong hệ thống. Sau khi đăng ký thành công, user sẽ tự động được gán role mặc định và nhận JWT token.

**Request:**
- Body:
```json
{
  ""username"": ""newuser"",
  ""fullName"": ""Nguyễn Văn A"",
  ""email"": ""user@example.com"",
  ""address"": ""123 Đường ABC"",
  ""phone"": ""0123456789"",
  ""password"": ""Password123!@#"",
  ""dateOfBirth"": ""2000-01-01""
}
```
  - `username` (string, required, max 50) - Tên đăng nhập (chỉ chứa chữ và số)
  - `fullName` (string, required, max 100) - Họ và tên
  - `email` (string, required, max 100) - Email (phải đúng format)
  - `address` (string, required, max 200) - Địa chỉ
  - `phone` (string, required) - Số điện thoại (bắt đầu bằng 0, 10-11 chữ số)
  - `password` (string, required, 8-100 ký tự) - Mật khẩu (phải có chữ hoa, chữ thường, số và ký tự đặc biệt)
  - `dateOfBirth` (DateTime, required) - Ngày sinh

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Registration successful"",
  ""statusCode"": 200,
  ""data"": {
    ""userID"": ""user-id-123"",
    ""userName"": ""newuser"",
    ""email"": ""user@example.com"",
    ""name"": ""Nguyễn Văn A"",
    ""address"": ""123 Đường ABC"",
    ""phone"": ""0123456789"",
    ""dateOfBirth"": ""2000-01-01"",
    ""isActive"": true,
    ""roles"": [""Student""],
    ""token"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."",
    ""refreshToken"": ""refresh-token-here""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Dữ liệu không hợp lệ"",
  ""errors"": [
    ""Username chỉ được chứa chữ và số"",
    ""Email không hợp lệ"",
    ""Mật khẩu phải có chữ hoa, chữ thường, số và ký tự đặc biệt""
  ]
}
```

2. **Username đã tồn tại:**
```json
{
  ""message"": ""Username already exists"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Email đã tồn tại:**
```json
{
  ""message"": ""Email already exists"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Sau khi đăng ký thành công, user tự động được gán role mặc định (Student)
- Token được trả về ngay sau khi đăng ký thành công, user có thể sử dụng ngay")]
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
        [SwaggerOperation(
            Summary = "Thiết lập role cho user", 
            Description = @"Api dùng để user tự thiết lập role cho chính mình. User có thể chọn một trong các role: Parent, Student, Teacher, Manager, Admin.

**Request:**
- Body:
```json
{
  ""userId"": ""user-id-123"",
  ""role"": ""Student""
}
```
  - `userId` (string, required) - ID của user cần thiết lập role
  - `role` (string, required) - Role cần thiết lập: `Parent`, `Student`, `Teacher`, `Manager`, hoặc `Admin`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Role set successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""userId"": ""user-id-123"",
    ""role"": ""Student"",
    ""message"": ""Role đã được thiết lập thành công""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Dữ liệu không hợp lệ"",
  ""statusCode"": 400,
  ""data"": ""Role phải là Parent, Student, Teacher, Manager hoặc Admin""
}
```

2. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng hiện tại"",
  ""statusCode"": 401,
  ""data"": null
}
```

3. **User không tồn tại:**
```json
{
  ""message"": ""User not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

4. **User ID không khớp:**
```json
{
  ""message"": ""You can only set role for yourself"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- User ID được lấy tự động từ JWT token
- Admin chỉ có thể thiết lập role
- Role phải là một trong các giá trị hợp lệ: Parent, Student, Teacher, Manager, Admin")]
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
        [SwaggerOperation(
            Summary = "Đổi mật khẩu", 
            Description = @"Api dùng để user đổi mật khẩu của mình. User cần cung cấp mật khẩu hiện tại và mật khẩu mới.

**Request:**
- Body:
```json
{
  ""userName"": ""username"",
  ""currentPassword"": ""OldPassword123!@#"",
  ""newPassword"": ""NewPassword123!@#"",
  ""confirmNewPassword"": ""NewPassword123!@#""
}
```
  - `userName` (string, required) - Tên đăng nhập
  - `currentPassword` (string, required) - Mật khẩu hiện tại
  - `newPassword` (string, required) - Mật khẩu mới (phải đáp ứng yêu cầu về độ mạnh)
  - `confirmNewPassword` (string, required) - Xác nhận mật khẩu mới (phải khớp với newPassword)

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Password changed successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""message"": ""Mật khẩu đã được thay đổi thành công"",
    ""userName"": ""username""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Mật khẩu hiện tại không đúng:**
```json
{
  ""message"": ""Current password is incorrect"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Mật khẩu mới không khớp:**
```json
{
  ""message"": ""New password and confirm password do not match"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Mật khẩu mới không đáp ứng yêu cầu:**
```json
{
  ""message"": ""New password must meet security requirements"",
  ""statusCode"": 400,
  ""data"": null
}
```

4. **User không tồn tại:**
```json
{
  ""message"": ""User not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Mật khẩu mới phải đáp ứng các yêu cầu về độ mạnh (chữ hoa, chữ thường, số, ký tự đặc biệt)
- Mật khẩu mới và xác nhận mật khẩu phải khớp nhau")]
        public async Task<BaseResponse<AccountChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordModel changePassword)
        {
            return await _accountService.ChangePassword(changePassword);
        }

        [HttpPost("link-student-to-parent")]
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(
            Summary = "Liên kết tài khoản học sinh với phụ huynh (Parent)", 
            Description = @"Api dùng để phụ huynh liên kết tài khoản học sinh với tài khoản của mình thông qua email của học sinh.

**Request:**
- Body: `studentEmail` (string) - Email của tài khoản học sinh cần liên kết
```json
""student@example.com""
```

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Student linked successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""parentId"": ""parent-id-123"",
    ""studentId"": ""student-id-456"",
    ""parentName"": ""Nguyễn Văn B"",
    ""studentName"": ""Nguyễn Văn A"",
    ""linkedAt"": ""2024-01-15T10:30:00Z""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không xác định được tài khoản."",
  ""statusCode"": 401,
  ""data"": null
}
```

2. **Email học sinh không tồn tại:**
```json
{
  ""message"": ""Student email not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Học sinh không có role Student:**
```json
{
  ""message"": ""User is not a student"",
  ""statusCode"": 400,
  ""data"": null
}
```

4. **Đã liên kết trước đó:**
```json
{
  ""message"": ""Student is already linked to this parent"",
  ""statusCode"": 400,
  ""data"": null
}
```

5. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Dữ liệu không hợp lệ"",
  ""statusCode"": 400,
  ""data"": ""Error details""
}
```

**Lưu ý:**
- Chỉ Parent role mới có quyền sử dụng API này
- Parent ID được lấy tự động từ JWT token
- Học sinh phải có role Student mới có thể được liên kết
- Một học sinh có thể được liên kết với nhiều phụ huynh")]
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
        [SwaggerOperation(
            Summary = "Lấy danh sách tất cả học sinh đã liên kết với Parent (Parent)", 
            Description = @"Api dùng để phụ huynh xem danh sách tất cả các học sinh đã được liên kết với tài khoản của mình.

**Request:**
- Không có query parameters

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Students retrieved successfully"",
  ""statusCode"": 200,
  ""data"": [
    {
      ""userID"": ""student-id-123"",
      ""userName"": ""student1"",
      ""email"": ""student1@example.com"",
      ""name"": ""Nguyễn Văn A"",
      ""address"": ""123 Đường ABC"",
      ""phone"": ""0123456789"",
      ""dateOfBirth"": ""2010-01-01"",
      ""isActive"": true,
      ""roles"": [""Student""]
    },
    {
      ""userID"": ""student-id-456"",
      ""userName"": ""student2"",
      ""email"": ""student2@example.com"",
      ""name"": ""Nguyễn Thị B"",
      ""address"": ""456 Đường XYZ"",
      ""phone"": ""0987654321"",
      ""dateOfBirth"": ""2011-05-15"",
      ""isActive"": true,
      ""roles"": [""Student""]
    }
  ]
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không xác định được tài khoản."",
  ""statusCode"": 401,
  ""data"": null
}
```

2. **Không có học sinh nào được liên kết:**
```json
{
  ""message"": ""No students linked"",
  ""statusCode"": 200,
  ""data"": []
}
```

**Lưu ý:**
- Chỉ Parent role mới có quyền sử dụng API này
- Parent ID được lấy tự động từ JWT token
- Chỉ trả về các học sinh đã được liên kết với parent này
- Nếu chưa có học sinh nào được liên kết, trả về mảng rỗng")]
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
