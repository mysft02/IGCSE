using BusinessObject.DTOs.Response;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.OAuth;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly TrelloOAuthService _trelloOAuthService;

        public OAuthController(TrelloOAuthService trelloOAuthService)
        {
            _trelloOAuthService = trelloOAuthService;
        }

        [HttpGet("trello/connect")]
        [SwaggerOperation(Summary = "Lấy URL OAuth Trello để kết nối tài khoản")]
        public async Task<ActionResult<BaseResponse<string>>> ConnectTrello()
        {
            var authUrl = _trelloOAuthService.connectTrello();
                
            return Ok(new BaseResponse<string>(
                "Trello OAuth URL được tạo thành công",
                Common.Constants.StatusCodeEnum.OK_200,
                authUrl));
        }

        [HttpGet("trello/callback")]
        [Authorize(Roles = "Teacher, Manager")]
        [SwaggerOperation(Summary = "Callback từ Trello")]
        public async Task<ActionResult<BaseResponse<string>>> CallbackTrello([FromQuery] string token)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }
            
            await _trelloOAuthService.callbackTrello(userId, token);
            return Ok(new BaseResponse<string>(
                "Trello OAuth Token kết nối thành công",
                Common.Constants.StatusCodeEnum.OK_200,
                null));
        }
    }
}