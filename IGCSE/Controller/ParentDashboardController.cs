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
    [Route("api/parent-dashboard")]
    [ApiController]
    [Authorize(Roles = "Parent")]
    public class ParentDashboardController : BaseAnalyticsController
    {
        private readonly CourseService _courseService;

        public ParentDashboardController(CourseService courseService)
        {
            _courseService = courseService;
        }

        private const string GetChildLearningProgressSummary = "Lấy tiến độ học tập của các con theo thời gian";
        private const string GetChildCourseStatisticsSummary = "Lấy thống kê khoá học của các con";
        private const string GetChildPerformanceSummary = "Lấy thành tích học tập (quiz scores) của các con";
        private const string GetChildCertificatesSummary = "Lấy danh sách chứng chỉ của các con";

        /// <summary>
        /// Lấy tiến độ học tập của các con theo thời gian
        /// </summary>
        [HttpGet("child-learning-progress")]
        [SwaggerOperation(Summary = GetChildLearningProgressSummary)]
        [SwaggerResponse(200, "Thành công", typeof(ParentChildLearningProgressResponse))]
        [SwaggerResponse(401, "Chưa đăng nhập")]
        [SwaggerResponse(403, "Không có quyền truy cập")]
        public async Task<IActionResult> GetChildLearningProgress(
            [FromQuery] AnalyticsDateRangeRequest? dateRange = null)
        {
            var parentId = ValidateUser();
            var result = await _courseService.GetParentChildLearningProgressAsync(parentId, dateRange);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê khoá học của các con
        /// </summary>
        [HttpGet("child-course-statistics")]
        [SwaggerOperation(Summary = GetChildCourseStatisticsSummary)]
        [SwaggerResponse(200, "Thành công", typeof(ParentChildCourseStatisticsResponse))]
        [SwaggerResponse(401, "Chưa đăng nhập")]
        [SwaggerResponse(403, "Không có quyền truy cập")]
        public async Task<IActionResult> GetChildCourseStatistics()
        {
            var parentId = ValidateUser();
            var result = await _courseService.GetParentChildCourseStatisticsAsync(parentId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thành tích học tập (quiz scores) của các con
        /// </summary>
        [HttpGet("child-performance")]
        [SwaggerOperation(Summary = GetChildPerformanceSummary)]
        [SwaggerResponse(200, "Thành công", typeof(ParentChildPerformanceResponse))]
        [SwaggerResponse(401, "Chưa đăng nhập")]
        [SwaggerResponse(403, "Không có quyền truy cập")]
        public async Task<IActionResult> GetChildPerformance()
        {
            var parentId = ValidateUser();
            var result = await _courseService.GetParentChildPerformanceAsync(parentId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách chứng chỉ của các con
        /// </summary>
        [HttpGet("child-certificates")]
        [SwaggerOperation(Summary = GetChildCertificatesSummary)]
        [SwaggerResponse(200, "Thành công", typeof(ParentChildCertificateResponse))]
        [SwaggerResponse(401, "Chưa đăng nhập")]
        [SwaggerResponse(403, "Không có quyền truy cập")]
        public async Task<IActionResult> GetChildCertificates()
        {
            var parentId = ValidateUser();
            var result = await _courseService.GetParentChildCertificatesAsync(parentId);
            return Ok(result);
        }
    }
}

