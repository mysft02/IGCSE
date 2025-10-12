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

        public TestController(TestService testService)
        {
            _testService = testService;
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

            var result = await _testService.MarkTest(request.Questions);
            return Ok(result);
        }
    }
}
