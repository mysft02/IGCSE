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
            Description = @"Api dùng để giáo viên xem thống kê chi tiết về một khóa học của mình, bao gồm số lượng học sinh, điểm số trung bình và thu nhập.

**Request:**
- Query parameter: `courseId` (int, required) - ID của khóa học cần xem thống kê

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course analytics retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""courseName"": ""Tên khóa học"",
    ""totalStudents"": 150,
    ""averageScore"": 8.5,
    ""totalRevenue"": 150000000,
    ""revenueByDate"": [
      {
        ""date"": ""2024-01-15"",
        ""revenue"": 5000000,
        ""enrollments"": 5
      }
    ]
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **Khóa học không tồn tại:**
```json
{
  ""message"": ""Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Khóa học không thuộc về giáo viên này:**
```json
{
  ""message"": ""You don't have permission to view this course analytics"",
  ""statusCode"": 403,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Teacher ID được lấy tự động từ JWT token
- Chỉ có thể xem thống kê của các khóa học do chính teacher đó tạo
- `revenueByDate` hiển thị doanh thu theo ngày (các khóa học được mua trong ngày)")]
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
