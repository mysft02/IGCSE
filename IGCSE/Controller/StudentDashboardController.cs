using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.Dashboards;
using BusinessObject.Payload.Request.Filter;
using Common.Utils;
using IGCSE.Controller.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace IGCSE.Controller
{
    [Route("api/student-dashboard")]
    [ApiController]
    public class StudentDashboardController : BaseAnalyticsController
    {
        private readonly CourseService _courseService;

        public StudentDashboardController(CourseService courseService)
        {
            _courseService = courseService;
        }

        private const string GetLearningProgressSummary = "Lấy thống kê tiến độ học tập cho học viên (Student)";
        private const string GetLearningProgressDescription = @"Api lấy thống kê tiến độ học tập của học viên theo thời gian. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

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
  ""message"": ""Lấy thống kê tiến độ học tập thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""timeUnit"": ""Month"",
      ""totalCompletedLessonItems"": 150,
      ""totalStudyHours"": 45.5,
      ""activeCourses"": 5
    },
    ""chart"": [
      {
        ""label"": ""T1"",
        ""completedLessonItems"": 10,
        ""studyHours"": 3.5
      },
      {
        ""label"": ""T2"",
        ""completedLessonItems"": 15,
        ""studyHours"": 5.2
      }
    ]
  }
}
```";

        private const string GetCourseStatisticsSummary = "Lấy thống kê khoá học cho học viên (Student)";
        private const string GetCourseStatisticsDescription = @"Api lấy thống kê tổng quan về các khoá học của học viên.

**Response Schema (Pattern chung: Metadata + Chart):**
```json
{
  ""message"": ""Lấy thống kê khoá học thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""totalEnrolledCourses"": 10,
      ""completedCourses"": 3,
      ""inProgressCourses"": 5,
      ""notStartedCourses"": 2,
      ""averageCompletionPercentage"": 65.5
    },
    ""chart"": [
      {
        ""label"": ""Khóa học ABC"",
        ""courseId"": 34,
        ""courseName"": ""Khóa học ABC"",
        ""completionPercentage"": 100.0,
        ""completedLessonItems"": 50,
        ""totalLessonItems"": 50,
        ""status"": ""Completed""
      },
      {
        ""label"": ""Khóa học XYZ"",
        ""courseId"": 35,
        ""courseName"": ""Khóa học XYZ"",
        ""completionPercentage"": 75.5,
        ""completedLessonItems"": 30,
        ""totalLessonItems"": 40,
        ""status"": ""InProgress""
      }
    ]
  }
}
```";

        [HttpGet("get-learning-progress")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(
            Summary = GetLearningProgressSummary,
            Description = GetLearningProgressDescription)]
        public async Task<ActionResult<BaseResponse<StudentLearningProgressResponse>>> GetLearningProgress(
            [FromQuery] AnalyticsDateRangeRequest? dateRange = null)
        {
            var userId = ValidateUser();
            var result = await _courseService.GetStudentLearningProgressAsync(userId, dateRange);
            return Ok(result);
        }

        [HttpGet("get-course-statistics")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(
            Summary = GetCourseStatisticsSummary,
            Description = GetCourseStatisticsDescription)]
        public async Task<ActionResult<BaseResponse<StudentCourseStatisticsResponse>>> GetCourseStatistics()
        {
            var userId = ValidateUser();
            var result = await _courseService.GetStudentCourseStatisticsAsync(userId);
            return Ok(result);
        }

        [HttpGet("get-course-recommendations")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(
            Summary = "Lấy danh sách khoá học gợi ý cho học viên",
            Description = @"Api gợi ý các khoá học phù hợp cho học viên dựa trên:
- Embedding similarity với các khoá học đã học
- Số lượng chứng chỉ đã có
- Số lượng bài học đã hoàn thành
- Điểm trung bình các quiz/final quiz

**Request Parameters (Query):**
- `limit` (int, optional) - Số lượng khoá học gợi ý. Mặc định: 10

**Response Schema:**
```json
{
  ""message"": ""Lấy danh sách khoá học gợi ý thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""totalRecommended"": 10,
    ""studentAchievement"": {
      ""completedCourses"": 5,
      ""totalCertificates"": 3,
      ""totalCompletedLessonItems"": 150,
      ""averageScore"": 8.5
    },
    ""recommendedCourses"": [
      {
        ""courseId"": 34,
        ""courseName"": ""Khóa học ABC"",
        ""courseDescription"": ""Mô tả khóa học"",
        ""imageUrl"": ""https://example.com/image.jpg"",
        ""price"": 1000000,
        ""similarityScore"": 0.85,
        ""recommendationScore"": 0.92,
        ""recommendationReason"": ""Tương đồng với các khoá học bạn đã học, Phù hợp với trình độ của bạn""
      }
    ]
  }
}
```")]
        public async Task<ActionResult<BaseResponse<BusinessObject.DTOs.Response.Dashboards.CourseRecommendationResponse>>> GetCourseRecommendations(
            [FromQuery] int limit = 10)
        {
            var userId = ValidateUser();
            var result = await _courseService.GetCourseRecommendationsAsync(userId, limit);
            return Ok(result);
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
