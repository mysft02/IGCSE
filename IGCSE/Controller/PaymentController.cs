using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Payment;
using BusinessObject.Payload.Request.PayOS;
using BusinessObject.Payload.Response.PayOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

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

        // Course key endpoints removed

        [HttpGet("get-transaction-history")]
        public async Task<ActionResult<BaseResponse<IEnumerable<TransactionHistoryResponse>>>> GetAllTransactionHistories([FromQuery] string userId)
        {
            var result = await _paymentService.GetAllTransactionHistoriesByUserId(userId);
            return Ok(result);
        }

        
        [HttpGet("get-payos-payment-url")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PayOSApiResponse>>> GetPayOSPaymentUrl([FromQuery] PayOSPaymentRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _paymentService.GetPayOSPaymentUrlAsync(request, userId);
            return Ok(result);
        }

        [HttpPost("payment-callback")]
        [SwaggerOperation(Summary = "Xử lí giao dịch sau khi thanh toán")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PayOSPaymentReturnResponse>>> PaymentCallback()
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản học sinh.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var currentUrl = Request.GetDisplayUrl();

            var queryParams = Request.Query
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            var result = await _paymentService.HandlePaymentAsync(queryParams, userId);
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
