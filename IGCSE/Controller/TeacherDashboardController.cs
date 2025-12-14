using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.Dashboards;
using BusinessObject.Payload.Request.Filter;
using IGCSE.Controller.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/teacher-dashboard")]
    [ApiController]
    public class TeacherDashboardController : BaseAnalyticsController
    {
        private readonly CourseService _courseService;
        private readonly PaymentService _paymentService;

        public TeacherDashboardController(CourseService courseService, PaymentService paymentService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
        }

        private const string GetCourseAnalyticsSummary = "Lấy thống kê khoá học cho giáo viên (Teacher)";
        private const string GetCourseAnalyticsDescription = @"Api dùng để giáo viên xem thống kê chi tiết về một khóa học của mình, bao gồm số lượng học sinh, điểm số trung bình và thu nhập.

**Request:**
- Query parameter: `courseId` (int, required) - ID của khóa học cần xem thống kê

**Response Schema:**
```json
{
  ""message"": ""Lấy thống kê khoá học thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""courseName"": ""Tên khóa học"",
    ""courseDescription"": ""Mô tả khóa học"",
    ""status"": ""Open"",
    ""price"": 1000000,
    ""imageUrl"": ""https://example.com/image.jpg"",
    ""createdAt"": ""2024-01-01T00:00:00"",
    ""updatedAt"": ""2024-01-01T00:00:00"",
    ""customerCount"": 150,
    ""averageFinalScore"": 8.5,
    ""totalIncome"": 150000000
  }
}
```

**Error Scenarios:**

**Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Khóa học không tồn tại:**
```json
{
  ""message"": ""Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Không có quyền truy cập:**
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
- Chỉ có thể xem thống kê của các khóa học do chính teacher đó tạo";

        [HttpGet("get-courses-analytics")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = GetCourseAnalyticsSummary,
            Description = GetCourseAnalyticsDescription)]
        public async Task<ActionResult<BaseResponse<CourseAnalyticsResponse>>> GetCourseAnalytics([FromQuery] int courseId)
        {
            return await HandleAnalyticsAsync(
                courseId,
                _courseService.GetCourseAnalyticsAsync
            );
        }

        private const string GetTeacherAnalyticsSummary = "Lấy thống kê tài khoản cho giáo viên (Teacher)";
        private const string GetTeacherAnalyticsDescription = @"Api lấy thống kê khoá học và thu nhập của giáo viên. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

**Request Parameters (Query):**
- `startDate` (DateTime, optional) - Ngày bắt đầu (YYYY-MM-DD). Mặc định: đầu năm hiện tại
- `endDate` (DateTime, optional) - Ngày kết thúc (YYYY-MM-DD). Mặc định: cuối năm hiện tại

**Time Unit tự động:**
- Nếu khoảng thời gian <= 24 giờ: Group theo **Giờ**
- Nếu khoảng thời gian <= 31 ngày: Group theo **Ngày**
- Nếu khoảng thời gian > 31 ngày: Group theo **Tháng**

**Lưu ý:** startDate và endDate chỉ filter cho thu nhập (income), không filter cho thống kê course.

**Response Schema (Pattern chung: Metadata + Chart):**
```json
{
  ""message"": ""Lấy thống kê cho giáo viên thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""totalCourses"": 40,
      ""pending"": 9,
      ""open"": 21,
      ""rejected"": 9,
      ""draft"": 1
    },
    ""chart"": [
      {
        ""label"": ""Tháng 1"",
        ""month"": 1,
        ""totalIncome"": 0,
        ""receivedIncome"": 0
      },
      {
        ""label"": ""Tháng 2"",
        ""month"": 2,
        ""totalIncome"": 0,
        ""receivedIncome"": 0
      },
      {
        ""label"": ""Tháng 11"",
        ""month"": 11,
        ""totalIncome"": 177000,
        ""receivedIncome"": 53100
      },
      {
        ""label"": ""Tháng 12"",
        ""month"": 12,
        ""totalIncome"": 196000,
        ""receivedIncome"": 58800
      }
    ]
  }
}
```

**Error Scenarios:**

**Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Teacher ID được lấy tự động từ JWT token
- `incomeCount` chứa dữ liệu thu nhập theo tháng (1-12) của năm hiện tại";

        [HttpGet("get-teacher-analytics")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = GetTeacherAnalyticsSummary,
            Description = GetTeacherAnalyticsDescription)]
        public async Task<ActionResult<BaseResponse<TeacherAnalyticsResponse>>> GetTotalAnalytics(
            [FromQuery] BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            var userId = ValidateUser();
            var result = await _courseService.GetTeacherAnalytics(userId, dateRange);
            return Ok(result);
        }

        private const string GetStudentLearningAbilitySummary = "Lấy thống kê khả năng học của học viên (Teacher)";
        private const string GetStudentLearningAbilityDescription = @"Api lấy thống kê khả năng học của học viên dựa trên số course mà các học viên học hoàn thành (được ghi vào bảng Processitem).

**Request Parameters (Query):**
- `courseId` (int, optional) - Filter theo Course ID cụ thể
- `studentId` (string, optional) - Filter theo Student ID cụ thể
- `startDate` (DateTime, optional) - Filter theo ngày bắt đầu
- `endDate` (DateTime, optional) - Filter theo ngày kết thúc
- `onlyCompletedCourses` (bool, optional) - Chỉ lấy học viên đã hoàn thành course (completion = 100%)
- `minCompletionPercentage` (decimal, optional) - Minimum completion percentage để filter (0-100)
- `page` (int, default: 0) - Số trang
- `size` (int, default: 10) - Số lượng records mỗi trang

**Response Schema:**
```json
{
  ""message"": ""Lấy thống kê khả năng học của học viên thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""totalStudents"": 25,
    ""students"": [
      {
        ""studentId"": ""user123"",
        ""studentName"": ""Nguyễn Văn A"",
        ""studentEmail"": ""nguyenvana@example.com"",
        ""totalEnrolledCourses"": 5,
        ""completedCourses"": 3,
        ""inProgressCourses"": 2,
        ""averageCompletionPercentage"": 75.5,
        ""courseDetails"": [
          {
            ""courseId"": 34,
            ""courseName"": ""Tên khóa học"",
            ""totalLessonItems"": 50,
            ""completedLessonItems"": 45,
            ""completionPercentage"": 90.0,
            ""isCompleted"": false,
            ""startedAt"": ""2024-01-15T00:00:00"",
            ""completedAt"": null
          },
          {
            ""courseId"": 35,
            ""courseName"": ""Khóa học khác"",
            ""totalLessonItems"": 30,
            ""completedLessonItems"": 30,
            ""completionPercentage"": 100.0,
            ""isCompleted"": true,
            ""startedAt"": ""2024-01-10T00:00:00"",
            ""completedAt"": ""2024-02-20T00:00:00""
          }
        ]
      }
    ],
    ""overall"": {
      ""averageCompletionPercentage"": 68.5,
      ""totalCompletedCourses"": 45,
      ""totalInProgressCourses"": 30,
      ""studentsWithCompletedCourses"": 20,
      ""studentsWithNoCompletedCourses"": 5
    }
  }
}
```

**Error Scenarios:**

**Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Teacher ID được lấy tự động từ JWT token
- Chỉ hiển thị thống kê của học viên trong các course do teacher này tạo
- Completion percentage được tính dựa trên số lesson items đã hoàn thành / tổng số lesson items trong course
- Course được coi là hoàn thành khi completion percentage = 100%";

        [HttpGet("get-student-learning-ability")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = GetStudentLearningAbilitySummary,
            Description = GetStudentLearningAbilityDescription)]
        public async Task<ActionResult<BaseResponse<StudentLearningAbilityResponse>>> GetStudentLearningAbility(
            [FromQuery] StudentLearningAbilityQueryRequest query)
        {
            // Set TeacherId từ JWT token
            var userId = ValidateUser();
            query.TeacherId = userId;

            var result = await _courseService.GetStudentLearningAbilityAsync(query);
            return Ok(result);
        }

        private const string GetLessonCompletionSummary = "Lấy thống kê hoàn thành lesson (Teacher)";
        private const string GetLessonCompletionDescription = @"Api lấy thống kê số lượng người hoàn thiện các lesson của giáo viên. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

**Request Parameters (Query):**
- `startDate` (DateTime, optional) - Ngày bắt đầu (YYYY-MM-DD). Mặc định: đầu năm hiện tại
- `endDate` (DateTime, optional) - Ngày kết thúc (YYYY-MM-DD). Mặc định: cuối năm hiện tại

**Time Unit tự động:**
- Nếu khoảng thời gian <= 24 giờ: Group theo **Giờ**
- Nếu khoảng thời gian <= 31 ngày: Group theo **Ngày**
- Nếu khoảng thời gian > 31 ngày: Group theo **Tháng**

**Response Schema (Pattern chung: Metadata + Chart):**
```json
{
  ""message"": ""Lấy thống kê hoàn thành lesson thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""timeUnit"": ""Month"",
      ""totalCompletedUsers"": 30,
      ""totalCompletedLessonItems"": 270
    },
    ""chart"": [
      {
        ""label"": ""2024-01"",
        ""completedUsersCount"": 0,
        ""completedLessonItemsCount"": 0
      },
      {
        ""label"": ""2024-02"",
        ""completedUsersCount"": 5,
        ""completedLessonItemsCount"": 30
      },
      {
        ""label"": ""2024-12"",
        ""completedUsersCount"": 25,
        ""completedLessonItemsCount"": 150
      }
    ]
  }
}
```

**Error Scenarios:**

**Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Teacher ID được lấy tự động từ JWT token
- Chỉ hiển thị thống kê của học viên trong các course do teacher này tạo
- Nếu không có startDate/endDate, mặc định là đầu năm và cuối năm hiện tại
- Time unit được tự động xác định dựa trên khoảng thời gian
- Tất cả time slots trong khoảng thời gian đều được trả về (kể cả không có dữ liệu = 0)";

        [HttpGet("get-lesson-completion")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = GetLessonCompletionSummary,
            Description = GetLessonCompletionDescription)]
        public async Task<ActionResult<BaseResponse<BusinessObject.DTOs.Response.Dashboards.LessonCompletionResponse>>> GetLessonCompletion(
            [FromQuery] BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest query)
        {
            // Set TeacherId từ JWT token
            var userId = ValidateUser();

            // startDate và endDate là optional, sẽ được set mặc định trong repository

            var result = await _courseService.GetLessonCompletionAsync(query, userId);
            return Ok(result);
        }

        [HttpGet("get-top-analytics")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Lấy danh sách top học viên tiềm năng và top khoá học được học nhiều nhất",
            Description = @"Api lấy danh sách top học viên tiềm năng nhất và top khoá học được học nhiều nhất trong một response.

**Top Students** được sắp xếp theo:
- Số course đã hoàn thành
- Tỷ lệ hoàn thành trung bình
- Tổng số lesson items đã hoàn thành
- Điểm trung bình các quiz/final quiz

**Top Courses** được sắp xếp theo:
- Số học viên đã đăng ký
- Số học viên đang học (active)
- Số học viên đã hoàn thành
- Tỷ lệ hoàn thành trung bình

**Request Parameters (Query):**
- `limit` (int, optional) - Số lượng học viên/khoá học cần lấy. Mặc định: 10

**Response Schema:**
```json
{
  ""message"": ""Lấy danh sách top học viên và khoá học thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""totalStudents"": 150,
    ""topStudents"": [
      {
        ""studentId"": ""user123"",
        ""studentName"": ""Nguyễn Văn A"",
        ""studentEmail"": ""a@example.com"",
        ""totalEnrolledCourses"": 5,
        ""completedCourses"": 3,
        ""averageCompletionPercentage"": 85.5,
        ""totalCompletedLessonItems"": 120,
        ""averageScore"": 8.5
      }
    ],
    ""totalCourses"": 50,
    ""topCourses"": [
      {
        ""courseId"": 34,
        ""courseName"": ""Khóa học ABC"",
        ""courseDescription"": ""Mô tả khóa học"",
        ""imageUrl"": ""https://example.com/image.jpg"",
        ""price"": 1000000,
        ""totalEnrollments"": 150,
        ""activeStudents"": 120,
        ""completedStudents"": 80,
        ""averageCompletionPercentage"": 75.5,
        ""averageFinalScore"": 8.2,
        ""totalIncome"": 150000000
      }
    ]
  }
}
```")]
        public async Task<ActionResult<BaseResponse<BusinessObject.DTOs.Response.Dashboards.TopAnalyticsResponse>>> GetTopAnalytics(
            [FromQuery] int limit = 10)
        {
            var userId = ValidateUser();
            var result = await _courseService.GetTopAnalyticsAsync(userId, limit);
            return Ok(result);
        }
    }
}
