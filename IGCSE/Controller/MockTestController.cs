using BusinessObject.DTOs.Request.MockTest;
using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.Payload.Request.MockTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/mocktest")]
    [ApiController]
    public class MockTestController : ControllerBase
    {
        private readonly MockTestService _mockTestService;

        public MockTestController(MockTestService mockTestService)
        {
            _mockTestService = mockTestService;
        }

        [HttpGet("get-all-mocktest")]
        [SwaggerOperation(Summary = "Lấy danh sách mock test")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<MockTestResponse>>>> GetAllMockTests([FromQuery] MockTestQueryRequest request)
        {
            var result = await _mockTestService.GetAllMockTestAsync(request);
            return Ok(result);
        }

        [HttpGet("get-mocktest-by-id")]
        [SwaggerOperation(Summary = "Lấy danh sách mock test theo id")]
        public async Task<ActionResult<BaseResponse<MockTestResponse>>> GetMockTestById([FromQuery] int id)
        {
            var result = await _mockTestService.GetMockTestByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("mark-mocktest")]
        [SwaggerOperation(Summary = "Chấm bài mock test")]
        public async Task<ActionResult<BaseResponse<List<QuizMarkResponse>>>> MarkMockTest([FromBody] QuizMarkRequest request)
        {
            var result = await _mockTestService.MarkMockTestAsync(request);
            return Ok(result);
        }

        [HttpPost("import-from-excel")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Import mock test từ file Excel")]
        public async Task<ActionResult<BaseResponse<MockTestCreateResponse>>> ImportFromExcel([FromForm] MockTestCreateRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _mockTestService.ImportMockTestFromExcelAsync(request, userId);
            return Ok(result);
        }

    }
}
