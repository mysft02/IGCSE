using BusinessObject.DTOs.Response.Dashboards;
using BusinessObject.DTOs.Response;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/teacher-dashboard")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly PaymentService _paymentService;

        public AdminDashboardController(CourseService courseService, PaymentService paymentService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
        }

        [HttpGet("get-admin-analytics")]
        [Authorize(Roles = "Admin, Manager")]
        [SwaggerOperation(
            Summary = "Lấy thống kê tài khoản cho admin",
            Description = @"Api lấy thống kê khoá học, tài khoản và thu nhập của admin:

**Response trả về `AdminAnalyticsResponse`:**
```json
{
  ""message"": ""Lấy thống kê cho admin thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""course"": {
      ""totalCourses"": 40,
      ""pending"": 9,
      ""open"": 21,
      ""rejected"": 9,
      ""draft"": 1
    },
    ""income"": {
      ""incomeCount"": [
        {
          ""month"": 1,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 2,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 3,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 4,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 5,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 6,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 7,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 8,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 9,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 10,
          ""totalIncome"": 0,
          ""receivedIncome"": 0
        },
        {
          ""month"": 11,
          ""totalIncome"": 177000,
          ""receivedIncome"": 53100
        },
        {
          ""month"": 12,
          ""totalIncome"": 196000,
          ""receivedIncome"": 58800
        }
      ]
    },
    ""accounts"": {
      ""totalAccounts"": 29,
      ""teacher"": 6,
      ""parent"": 3,
      ""student"": 16
    }
  }
}
```
            ")]
        public async Task<ActionResult<BaseResponse<AdminAnalyticsResponse>>> GetTotalAnalytics()
        {
            var result = await _courseService.GetAdminAnalytics();
            return Ok(result);
        }
    }
}
