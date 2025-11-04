using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response.Quizzes;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Common.Utils;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Quizzes;

namespace IGCSE.Controller
{
    [Route("api/quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;

        public QuizController(QuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet("get-all-quiz")]
        [SwaggerOperation(Summary = "Lấy toàn bộ danh sách quiz (có phân trang)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<Quiz>>>> GetAllQuizAsync([FromQuery] QuizQueryRequest request)
        {
            var result = await _quizService.GetAllQuizAsync(request);
            return Ok(result);
        }

        [HttpGet("get-quiz-by-id")]
        [SwaggerOperation(Summary = "Lấy danh sách quiz theo id")]
        public async Task<ActionResult<BaseResponse<QuizResponse>>> GetQuizById([FromQuery] int id)
        {
            var result = await _quizService.GetQuizByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("mark-quiz")]
        [Authorize]
        [SwaggerOperation(Summary = "Chấm bài quiz")]
        public async Task<ActionResult<BaseResponse<List<QuizMarkResponse>>>> MarkQuiz([FromBody] QuizMarkRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.isEmtyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _quizService.MarkQuizAsync(request, userId);
            return Ok(result);
        }
    }
}
