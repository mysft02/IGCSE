using BusinessObject.DTOs.Response;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller.Base
{
    /// <summary>
    /// Base controller cho các Analytics APIs
    /// Cung cấp các methods chung để xử lý analytics endpoints
    /// </summary>
    [ApiController]
    public abstract class BaseAnalyticsController : ControllerBase
    {
        /// <summary>
        /// Lấy UserId từ JWT token
        /// </summary>
        protected string? GetUserId()
        {
            return HttpContext.User.FindFirst("AccountID")?.Value;
        }

        /// <summary>
        /// Validate user và throw exception nếu không tìm thấy
        /// </summary>
        protected string ValidateUser()
        {
            var userId = GetUserId();
            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }
            return userId;
        }

        /// <summary>
        /// Generic method để xử lý analytics endpoint
        /// </summary>
        protected async Task<ActionResult<BaseResponse<TResponse>>> HandleAnalyticsAsync<TResponse>(
            Func<Task<BaseResponse<TResponse>>> serviceMethod)
        {
            var result = await serviceMethod();
            return Ok(result);
        }

        /// <summary>
        /// Generic method để xử lý analytics endpoint với userId
        /// </summary>
        protected async Task<ActionResult<BaseResponse<TResponse>>> HandleAnalyticsAsync<TResponse>(
            Func<string, Task<BaseResponse<TResponse>>> serviceMethod)
        {
            var userId = ValidateUser();
            var result = await serviceMethod(userId);
            return Ok(result);
        }

        /// <summary>
        /// Generic method để xử lý analytics endpoint với parameter
        /// </summary>
        protected async Task<ActionResult<BaseResponse<TResponse>>> HandleAnalyticsAsync<TParam, TResponse>(
            TParam param,
            Func<TParam, Task<BaseResponse<TResponse>>> serviceMethod)
        {
            var userId = ValidateUser();
            var result = await serviceMethod(param);
            return Ok(result);
        }

        /// <summary>
        /// Generic method để xử lý analytics endpoint với userId và parameter
        /// </summary>
        protected async Task<ActionResult<BaseResponse<TResponse>>> HandleAnalyticsAsync<TParam, TResponse>(
            TParam param,
            Func<string, TParam, Task<BaseResponse<TResponse>>> serviceMethod)
        {
            var userId = ValidateUser();
            var result = await serviceMethod(userId, param);
            return Ok(result);
        }
    }
}

