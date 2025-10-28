using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.Payment;
using Microsoft.AspNetCore.Mvc;
using Service;

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

        [HttpGet("get-course-analytics")]
        public async Task<BaseResponse<CourseAnalyticsResponse>> GetCourseAnalytics()
        {
            var result = await _courseService.GetCourseAnalyticsAsync();
            return result;
        }

        [HttpGet("get-payment-analytics")]
        public async Task<BaseResponse<PaymentAnalyticsResponse>> GetPaymentAnalytics()
        {
            var result = await _paymentService.GetPaymentAnalyticsAsync();
            return result;
        }
    }
}
