using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Dashboards;
using BusinessObject.Payload.Request.Filter;
using IGCSE.Controller.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/manager-dashboard")]
    [ApiController]
    public class ManagerDashboardController : BaseAnalyticsController
    {
        private readonly CourseService _courseService;

        public ManagerDashboardController(CourseService courseService)
        {
            _courseService = courseService;
        }

        private const string GetOverviewSummary = "Lấy tổng quan dashboard cho Manager";
        private const string GetOverviewDescription = @"Api lấy tổng quan dashboard cho Manager với các thông tin:
- Tổng quan tài khoản (Total, Teachers, Students, Parents, NewThisMonth)
- Tổng quan courses (Total, Open, Pending, Rejected, NewThisMonth)
- Tổng quan doanh thu (Total, ThisMonth, LastMonth, TotalTransactions)
- Tổng quan đăng ký (Total, ThisMonth, ActiveStudents, CompletedStudents)

**Response Schema:**
```json
{
  ""message"": ""Lấy tổng quan dashboard thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""accounts"": {
      ""total"": 1000,
      ""teachers"": 50,
      ""students"": 900,
      ""parents"": 50,
      ""newThisMonth"": 50
    },
    ""courses"": {
      ""total"": 200,
      ""open"": 150,
      ""pending"": 30,
      ""rejected"": 20,
      ""newThisMonth"": 10
    },
    ""revenue"": {
      ""total"": 1000000000,
      ""thisMonth"": 50000000,
      ""lastMonth"": 45000000,
      ""totalTransactions"": 5000
    },
    ""enrollments"": {
      ""total"": 5000,
      ""thisMonth"": 200,
      ""activeStudents"": 3000,
      ""completedStudents"": 1000
    }
  }
}
```";

        private const string GetUserStatisticsSummary = "Lấy thống kê tài khoản theo thời gian cho Manager";
        private const string GetUserStatisticsDescription = @"Api lấy thống kê tài khoản theo thời gian cho Manager. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

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
  ""message"": ""Lấy thống kê tài khoản thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""timeUnit"": ""Month"",
      ""totalAccounts"": 1000,
      ""totalTeachers"": 50,
      ""totalStudents"": 900,
      ""totalParents"": 50
    },
    ""chart"": [
      {
        ""label"": ""T1"",
        ""newAccounts"": 50,
        ""newTeachers"": 5,
        ""newStudents"": 45
      }
    ]
  }
}
```";

        private const string GetCourseStatisticsSummary = "Lấy thống kê khoá học theo thời gian cho Manager";
        private const string GetCourseStatisticsDescription = @"Api lấy thống kê khoá học theo thời gian cho Manager. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

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
  ""message"": ""Lấy thống kê khoá học thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""timeUnit"": ""Month"",
      ""totalCourses"": 200,
      ""newCourses"": 10,
      ""openCourses"": 150,
      ""pendingCourses"": 30
    },
    ""chart"": [
      {
        ""label"": ""T1"",
        ""newCourses"": 5,
        ""approvedCourses"": 3
      }
    ]
  }
}
```";

        private const string GetRevenueStatisticsSummary = "Lấy thống kê doanh thu theo thời gian cho Manager";
        private const string GetRevenueStatisticsDescription = @"Api lấy thống kê doanh thu theo thời gian cho Manager. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

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
  ""message"": ""Lấy thống kê doanh thu thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""timeUnit"": ""Month"",
      ""totalRevenue"": 1000000000,
      ""totalTransactions"": 5000,
      ""averageTransactionAmount"": 200000
    },
    ""chart"": [
      {
        ""label"": ""T1"",
        ""revenue"": 50000000,
        ""transactionCount"": 250
      }
    ]
  }
}
```";

        private const string GetEnrollmentStatisticsSummary = "Lấy thống kê đăng ký theo thời gian cho Manager";
        private const string GetEnrollmentStatisticsDescription = @"Api lấy thống kê đăng ký theo thời gian cho Manager. Tự động phân tích khoảng thời gian và group theo Hour/Day/Month.

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
  ""message"": ""Lấy thống kê đăng ký thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""metadata"": {
      ""timeUnit"": ""Month"",
      ""totalEnrollments"": 5000,
      ""activeStudents"": 3000,
      ""completedStudents"": 1000
    },
    ""chart"": [
      {
        ""label"": ""T1"",
        ""newEnrollments"": 200,
        ""completedEnrollments"": 50
      }
    ]
  }
}
```";

        [HttpGet("get-overview")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = GetOverviewSummary,
            Description = GetOverviewDescription)]
        public async Task<ActionResult<BaseResponse<AdminOverviewResponse>>> GetOverview()
        {
            var result = await _courseService.GetAdminOverviewAsync();
            return Ok(result);
        }

        [HttpGet("get-user-statistics")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = GetUserStatisticsSummary,
            Description = GetUserStatisticsDescription)]
        public async Task<ActionResult<BaseResponse<AdminUserStatisticsResponse>>> GetUserStatistics(
            [FromQuery] AnalyticsDateRangeRequest? dateRange = null)
        {
            var result = await _courseService.GetAdminUserStatisticsAsync(dateRange);
            return Ok(result);
        }

        [HttpGet("get-course-statistics")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = GetCourseStatisticsSummary,
            Description = GetCourseStatisticsDescription)]
        public async Task<ActionResult<BaseResponse<AdminCourseStatisticsResponse>>> GetCourseStatistics(
            [FromQuery] AnalyticsDateRangeRequest? dateRange = null)
        {
            var result = await _courseService.GetAdminCourseStatisticsAsync(dateRange);
            return Ok(result);
        }

        [HttpGet("get-revenue-statistics")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = GetRevenueStatisticsSummary,
            Description = GetRevenueStatisticsDescription)]
        public async Task<ActionResult<BaseResponse<AdminRevenueStatisticsResponse>>> GetRevenueStatistics(
            [FromQuery] AnalyticsDateRangeRequest? dateRange = null)
        {
            var result = await _courseService.GetAdminRevenueStatisticsAsync(dateRange);
            return Ok(result);
        }

        [HttpGet("get-enrollment-statistics")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = GetEnrollmentStatisticsSummary,
            Description = GetEnrollmentStatisticsDescription)]
        public async Task<ActionResult<BaseResponse<AdminEnrollmentStatisticsResponse>>> GetEnrollmentStatistics(
            [FromQuery] AnalyticsDateRangeRequest? dateRange = null)
        {
            var result = await _courseService.GetAdminEnrollmentStatisticsAsync(dateRange);
            return Ok(result);
        }
    }
}
