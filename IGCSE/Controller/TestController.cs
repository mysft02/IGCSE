using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Response.OpenAI;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Service.OpenAI;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly TestService _testService;
        private readonly OpenAIEmbeddingsApiService _openAIEmbeddingsApiService;

        public TestController(TestService testService, OpenAIEmbeddingsApiService openAIEmbeddingsApiService)
        {
            _testService = testService;
            _openAIEmbeddingsApiService = openAIEmbeddingsApiService;

        }

        [HttpPost("mark")]
        [SwaggerOperation(Summary = "Chấm bài kiểm tra")]
        public async Task<ActionResult<BaseResponse<TestResponse>>> MarkTest([FromBody] TestRequest request)
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

            var result = await _testService.MarkTest(request.Questions);
            return Ok(result);
        }
    }
}
