using BusinessObject.DTOs.Request.Payments;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.Payload.Request.PayOS;
using BusinessObject.Payload.Response.PayOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace IGCSE.Controller
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("get-payos-payment-url")]
        [Authorize(Roles = "Teacher, Parent")]
        public async Task<ActionResult<BaseResponse<PayOSApiResponse>>> GetPayOSPaymentUrl([FromQuery] PayOSPaymentRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _paymentService.GetPayOSPaymentUrlAsync(request, userId, userRole);
            return Ok(result);
        }

        [HttpPost("payment-callback")]
        [SwaggerOperation(Summary = "Xử lí giao dịch sau khi thanh toán PayOS")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PayOSPaymentReturnResponse>>> PaymentCallback([FromBody] PaymentCallBackRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _paymentService.HandlePaymentAsync(request, userId, userRole);
            return Ok(result);
        }

        [HttpPost("payout")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PayOSApiResponse>>> PayOSPayout([FromForm] PayoutRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _paymentService.PayOSPayoutAsync(request, userId);
            return Ok(result);
        }

        [HttpPost("create-or-update-webhook-url")]
        //[Authorize]
        public async Task<ActionResult<BaseResponse<PayOSApiResponse>>> CreateOrUpdateWebhookurl([FromForm] string url)
        {
            //var user = HttpContext.User;
            //var userId = user.FindFirst("AccountID")?.Value;

            //if (string.IsNullOrEmpty(userId))
            //{
            //    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            //}

            var result = await _paymentService.CreateOrUpdateWebhookUrl(url);
            return Ok(result);
        }
    }
}
