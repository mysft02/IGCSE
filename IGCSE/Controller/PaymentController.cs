using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using DTOs.Response.Accounts;
using DTOs.Response.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

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
        [Authorize] // UNCOMMENTED - REQUIRED FOR AUTHENTICATION
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

        [HttpGet("vnpay-callback")]

        public async Task<IActionResult> VnPayCallback([FromQuery] VnPayCallbackRequest request)
        {
            if (string.IsNullOrEmpty(request.vnp_TxnRef) || string.IsNullOrEmpty(request.vnp_ResponseCode))
            {
                return Content("<html><body><h2>Thanh toán thất bại</h2><p>Dữ liệu chưa đủ!</p></body></html>", "text/html");
            }
            
            try
            {
                var result = await _paymentService.HandlePaymentSuccessAsync(request);
                var key = result.Data;
                return Content($@"
                    <html>
                        <head><title>Thanh toán thành công</title></head>
                        <body>
                            <h2>Thanh toán thành công!</h2>
                            <p>Mã key kích hoạt khoá học của bạn: <b>{key}</b></p>
                        </body>
                    </html>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($@"<html><body>
                    <h2>Thanh toán thất bại</h2>
                    <p>{ex.Message}</p>
                    </body></html>", "text/html");
            }
        }

        [HttpGet("parent-coursekeys")]
        [Authorize(Roles = "Parent")]
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
    }
}
