using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.DTOs.Response.Payment;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly PaymentService _paymentService;

        public DashboardController(CourseService courseService, PaymentService paymentService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
        }

        [HttpGet("get-courses-analytics-for-teachers")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy thống kê khoá học cho giáo viên",
            Description = "Tổng hợp toàn bộ khoá học của giáo viên bao gồm thông tin khoá học, số lượng học sinh, điểm số trung bình và thu nhập từ khoá học. " +
            "`CourseId` để lấy 1 thống kê 1 khoá học nhất định; " +
            "`Date` để thống kê những khoá học được mua trong ngày")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseDashboardQueryResponse>>>> GetCourseAnalytics([FromQuery] CourseDashboardQueryRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.isEmtyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            request.userID = userId;

            var result = await _courseService.GetCourseAnalyticsAsync(request);
            return Ok(result);
        }
    }
}
