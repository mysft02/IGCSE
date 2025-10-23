using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using DTOs.Response.Accounts;
using DTOs.Response.Courses;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("create-vnpay-url")]
        [Authorize]
        [SwaggerOperation(Summary = "Tạo thanh toán khóa học (Parent)")]
        public async Task<ActionResult<BaseResponse<PaymentResponse>>> CreatePaymentUrl([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                var result = await _paymentService.CreatePaymentUrlAsync(HttpContext, request);
                return Ok(result);
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
    }
}
