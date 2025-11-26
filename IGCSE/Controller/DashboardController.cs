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
        [SwaggerOperation(Summary = "Lấy thống kê khoá học cho giáo viên",
            Description = "Tổng hợp toàn bộ khoá học của giáo viên bao gồm thông tin khoá học, số lượng học sinh, điểm số trung bình và thu nhập từ khoá học. " +
            "`Date` để thống kê những khoá học được mua trong ngày")]
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
