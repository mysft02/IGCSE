using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Common.Utils;
using BusinessObject.DTOs.Request.FinalQuizzes;
using BusinessObject.DTOs.Response.FinalQuizzes;
using System.Security.Claims;

namespace IGCSE.Controller
{
    [Route("api/final-quiz")]
    [ApiController]
    public class FinalQuizController : ControllerBase
    {
        private readonly FinalQuizService _finalQuizService;

        public FinalQuizController(FinalQuizService finalQuizService)
        {
            _finalQuizService = finalQuizService;
        }

        [HttpGet("get-final-quiz-by-id")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy danh sách Final Quiz theo id")]
        public async Task<ActionResult<BaseResponse<object>>> GetFinalQuizById([FromQuery] int id)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _finalQuizService.GetFinalQuizByIdAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("mark-final-quiz")]
        [Authorize]
        [SwaggerOperation(Summary = "Chấm bài Final Quiz")]
        public async Task<ActionResult<BaseResponse<List<FinalQuizMarkResponse>>>> MarkFinalQuiz([FromBody] FinalQuizMarkRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _finalQuizService.MarkFinalQuizAsync(request, userId);
            return Ok(result);
        }
    }
}
