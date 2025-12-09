using BusinessObject.DTOs.Request.AptitudeTestQuestions;
using BusinessObject.DTOs.Request.LearningPaths;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.AptitudeTestQuestionResponse;
using BusinessObject.DTOs.Response.AptitudeTestQuestions;
using BusinessObject.DTOs.Response.LearningPaths;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace IGCSE.Controller
{
    [Route("api/learning-path")]
    [ApiController]
    public class LearningPathController : ControllerBase
    {
        private readonly LearningPathService _learningPathService;

        public LearningPathController(LearningPathService learningPathService)
        {
            _learningPathService = learningPathService;
        }

        [HttpPost("create-learning-path")]
        public async Task<ActionResult<BaseResponse<LearningPathResponse>>> CreateLearningPath([FromBody] LearningPathCreateRequest request)
        {
            var result = await _learningPathService.CreateLearningPathAsync(request);

            return Ok(result);
        }

        [HttpGet("get-learning-path")]
        public async Task<ActionResult<BaseResponse<LearningPathResponse>>> GetLearningPath([FromQuery] int id)
        {
            var result = await _learningPathService.GetLearningPath(id);

            return Ok(result);
        }

        [HttpGet("get-all-learning-path")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<LearningPathResponse>>>> GetAllLearningPath([FromQuery] LearningPathQuery query)
        {
            var result = await _learningPathService.GetAllLearningPath(query);

            return Ok(result);
        }

        [HttpPost("add-aptitude-test-question")]
        public async Task<ActionResult<BaseResponse<AptitudeTestQuestionCreateResponse>>> AddAptitudeTestQuestion([FromQuery] AptitudeTestQuestionCreateRequest request)
        {
            var result = await _learningPathService.AddAptitudeTestQuestionAsync(request);

            return Ok(result);
        }

        [HttpGet("get-aptitude-test")]
        public async Task<ActionResult<BaseResponse<AptitudeTestResponse>>> GetAptitudteTest()
        {
            var result = await _learningPathService.GetAptitudeTest();

            return Ok(result);
        }
    }
}
