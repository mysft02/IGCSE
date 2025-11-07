using BusinessObject.DTOs.Request.MockTest;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.Payload.Request.MockTest;
using Common.Utils;
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
        [Authorize]
        [SwaggerOperation(Summary = "Lấy mock test để học sinh thực hiện bài thi")]
        public async Task<ActionResult<BaseResponse<MockTestResponse>>> GetMockTestForStudent([FromQuery] int id)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.isEmtyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.GetMockTestByIdAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("mark-mocktest")]
        [Authorize]
        [SwaggerOperation(Summary = "Chấm bài mock test")]
        public async Task<ActionResult<BaseResponse<List<MockTestMarkResponse>>>> MarkMockTest([FromBody] MockTestMarkRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.isEmtyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.MarkMockTestAsync(request, userId);
            return Ok(result);
        }
    }
}
