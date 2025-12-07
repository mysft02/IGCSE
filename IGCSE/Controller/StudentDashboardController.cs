using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace IGCSE.Controller
{
    [Route("api/teacher-dashboard")]
    [ApiController]
    public class StudentDashboardController : ControllerBase
    {
        private readonly CourseService _courseService;

        public StudentDashboardController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("get-activity-count")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy dữ liệu hoạt động của student",
            Description = @"Api dùng để lấy dữ liệu hoạt động của student

**Request:**
- Query parameter: `studentId` (int, required) - ID của student cần xem thống kê

**Lưu ý:**
- api chỉ dùng cho role ""Parent"", ""Student""
- Nếu role ""Student"" k cần truyền ""studentId""
- Nếu role ""Parent"" cần truyền ""studentId"" của học sinh cần xem

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""string"",
  ""statusCode"": 100,
  ""data"": {
    ""date"": ""string"",
    ""count"": 0
  }
}
```

**Lưu ý:**
- Api sẽ trả về tần suất hoạt động của học sinh trong 365 ngày
- Nếu count = 0 nghĩa là không có hoạt động.")]
        public async Task<ActionResult<BaseResponse<ActivityCountResponse>>> GetActivityCount([FromQuery] string? studentId)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _courseService.GetStudentActivityCount(userId, userRole, studentId);
            return Ok(result);
        }
    }
}
