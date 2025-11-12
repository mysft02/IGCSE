using BusinessObject.DTOs.Request.MockTest;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.MockTest;
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
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lấy danh sách mock test", Description = "Lấy danh sách mock test với các trạng thái: " +
            "`1` là `Completed`(đã hoàn thành bài thi trước đó); " +
            "`2` là `Open`(đã mua gói nhưng chưa hoàn thành bài thi trước đó); " + 
            "`3` là `Locked`(chưa mua gói bài thi)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<MockTestResponse>>>> GetAllMockTests([FromQuery] MockTestQueryRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            request.userID = userId;

            var result = await _mockTestService.GetAllMockTestAsync(request);
            return Ok(result);
        }

        [HttpGet("get-mocktest-result")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy danh sách kết quả các bài thi mock test đã làm", 
            Description ="Api dùng để lấy danh sách toàn bộ kết quả bài mock test đã làm của người dùng có bao gồm đáp án, cách giải và câu trả lời của người dùng. " +
            "`MockTestId` để lọc theo danh sách bài mock test, " +
            "`MockTestResultId` để lấy 1 kết quả nhất định")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<MockTestResultQueryResponse>>>> GetAllMockTestResult([FromQuery] MockTestResultQueryRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            request.userID = userId;

            var result = await _mockTestService.GetAllMockTestResultAsync(request);
            return Ok(result);
        }

        [HttpGet("get-mocktest-by-id")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy mock test để học sinh thực hiện bài thi", Description ="Api dùng để lấy danh sách câu hỏi của bài mock test cho việc thực hiện bài mock test không bao gồm đáp án")]
        public async Task<ActionResult<BaseResponse<MockTestForStudentResponse>>> GetMockTestForStudent([FromQuery] int id)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
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

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.MarkMockTestAsync(request, userId);
            return Ok(result);
        }
    }
}
