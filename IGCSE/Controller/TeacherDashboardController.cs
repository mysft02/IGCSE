using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/teacher-dashboard")]
    [ApiController]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly PaymentService _paymentService;

        public TeacherDashboardController(CourseService courseService, PaymentService paymentService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
        }

        [HttpGet("get-courses-analytics")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Lấy thống kê khoá học cho giáo viên (Teacher)", 
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
        public async Task<ActionResult<BaseResponse<CourseAnalyticsResponse>>> GetCourseAnalytics([FromQuery] int courseId)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _courseService.GetCourseAnalyticsAsync(courseId);
            return Ok(result);
        }
    }
}
