using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Response.OpenAI;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Service.OpenAI;

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

            try
            {
                var result = await _testService.MarkTest(request.Questions);
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

        [HttpPost("embed")]
        public async Task<ActionResult<BaseResponse<OpenAIEmbeddingsApiResponse>>> EmbedData([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu đầu vào không được để trống",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    "Input data cannot be null"
                ));
            }

            try
            {
                var embeddingResult = await _openAIEmbeddingsApiService.EmbedData(course);

                return Ok(new BaseResponse<OpenAIEmbeddingsApiResponse>(
                    "Embedding được tạo thành công",
                    Common.Constants.StatusCodeEnum.Accepted_202,
                    embeddingResult
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
