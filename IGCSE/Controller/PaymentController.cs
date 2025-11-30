using BusinessObject.DTOs.Request.Payments;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.Payment;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
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
        [SwaggerOperation(Summary = "Lấy link thanh toán payos", Description = "Api dùng để tạo link thanh toán cho `course` hoặc `package`. " +
            "`Amount` là số tiền của sản phẩm. " +
            "`CourseId` là id của khoá học cần mua. " +
            "`PackageId` là id của package cần mua. " +
            "`DestUserId` là id của `Student` mà cần mua cho. Chỉ dùng khi `Parent` muốn mua course hoặc package cho `Student`, còn `Teacher` mua package thì k cần truyền vào. " +
            "`Lưu ý k truyền CourseId và PackageId cùng 1 lúc`.")]
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
        [SwaggerOperation(Summary = "Xử lí giao dịch sau khi thanh toán PayOS", Description = "Api dùng để xử lí sau khi PayOS return dữ liệu thanh toán về. " +
            "Cột `Code` là trạng thái thanh toán: `00` là thành công, còn lại là thất bại. " +
            "Cột `Id` là mã của giao dịch thanh toán payos. " +
            "Cột `Cancel` là trạng thái huỷ. Nếu không huỷ là `false`, còn lại là `true`.")]
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

        [HttpGet("get-transaction-history")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<TransactionHistoryResponse>>>> GetTransactionHistory([FromQuery] TransactionHistoryQueryRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            request.userID = userId;

            var result = await _paymentService.GetTransactionHistory(request);
            return Ok(result);
        }
    }
}
