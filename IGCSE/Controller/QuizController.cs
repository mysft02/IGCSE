using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response.Quizzes;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpGet("get-quiz-by-id")]
        [SwaggerOperation(Summary = "Lấy danh sách quiz theo id")]
        public async Task<ActionResult<BaseResponse<QuizResponse>>> GetQuizById([FromQuery] int id)
        {
            var result = await _quizService.GetQuizByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("mark-quiz")]
        [SwaggerOperation(Summary = "Chấm bài quiz")]
        public async Task<ActionResult<BaseResponse<List<QuizMarkResponse>>>> MarkQuiz([FromBody] QuizMarkRequest request)
        {
            var result = await _quizService.MarkQuizAsync(request);
            return Ok(result);
        }
    }
}
