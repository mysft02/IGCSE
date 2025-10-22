using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response.Quizzes;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Service;
using Common.Utils;

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
        public async Task<ActionResult<BaseResponse<QuizResponse>>> GetQuizById([FromQuery] int id)
        {
            var result = await _quizService.GetQuizByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("mark-quiz")]
        public async Task<ActionResult<BaseResponse<List<QuizMarkResponse>>>> MarkQuiz([FromBody] QuizMarkRequest request)
        {
            var result = await _quizService.MarkQuizAsync(request);
            return Ok(result);
        }

        [HttpPost("import-from-excel")]
        public async Task<ActionResult<BaseResponse<QuizCreateResponse>>> ImportFromExcel([FromForm] QuizCreateRequest request)
        {
            var result = await _quizService.ImportQuizFromExcelAsync(request);
            return Ok(result);
        }
    }
}
