using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
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

        [HttpGet("parent-coursekeys")]
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(Summary = "Lấy danh sách các key khóa học đã thanh toán thành công mà parent đang có (Parent)")]
        public async Task<ActionResult<BaseResponse<List<CourseKeyResponse>>>> GetParentCourseKeys()
        {
            try
            {
                var user = HttpContext.User;
                var parentId = user.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(parentId))
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));

                var keys = await _paymentService.GetCourseKeysByParentAsync(parentId);

                return Ok(new BaseResponse<List<CourseKeyResponse>>(
                    $"Tìm thấy {keys.Count} khóa học",
                    Common.Constants.StatusCodeEnum.OK_200,
                    keys
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("get-all-keys")]
        [SwaggerOperation(Summary = "Lấy tất cả key khóa học đã thanh toán thành công")]
        public async Task<ActionResult<BaseResponse<List<CourseKeyResponse>>>> GetAllCourseKeys([FromQuery] string? status = null, [FromQuery] string? parentId = null, [FromQuery] int? courseId = null)
        {
            try
            {
                var keys = await _paymentService.GetFilteredCourseKeysAsync(status, parentId, courseId);

                return Ok(new BaseResponse<List<CourseKeyResponse>>(
                    $"Tìm thấy {keys.Count} mã khóa học",
                    Common.Constants.StatusCodeEnum.OK_200,
                    keys
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("get-transaction-history")]
        public async Task<ActionResult<BaseResponse<List<CourseKeyResponse>>>> GetAllTransactionHistories([FromQuery] string userId)
        {
            var result = _paymentService.GetAllTransactionHistoriesByUserId(userId);
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
        [SwaggerOperation(Summary = "Xử lí giao dịch sau khi thanh toán PayOS")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PayOSPaymentReturnResponse>>> PaymentCallback([FromBody] Dictionary<string, string>? queryParams = null)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            if (queryParams == null || queryParams.Count == 0)
            {
                queryParams = Request.Query
                    .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
            }

            if (queryParams == null || queryParams.Count == 0)
            {
                return BadRequest(new BaseResponse<string>("Không tìm thấy thông tin thanh toán từ PayOS.", Common.Constants.StatusCodeEnum.BadRequest_400, null));
            }

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
