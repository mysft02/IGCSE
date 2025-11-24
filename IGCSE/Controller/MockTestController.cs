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

        [HttpGet("get-mocktest-by-id")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy mock test để học sinh thực hiện bài thi hoặc xem kết quả", 
            Description = @"Api tự động trả về response khác nhau dựa trên trạng thái của mock test:

            **1. Trường hợp: Mock test chưa unlock (Locked - Status = 3)**
            - Trả về lỗi: ""Bạn chưa đăng kí gói thi thử.""
            - StatusCode: BadRequest_400

            **2. Trường hợp: Mock test đã unlock nhưng chưa làm (Open - Status = 2)**
            - Response type: `MockTestForStudentResponse`
            - Schema:
            ```json
            {
            ""message"": ""Lấy mock test thành công"",
            ""statusCode"": 200,
            ""data"": {: ""2024-01-01T00:00:00Z"",
                ""updatedAt"": ""2024-01-01T00:00:00Z"",
                ""createdBy"": ""userId"",
                ""mockTestQuestions"": [
                {
                    ""mockTestQuestionId"": 1,
                    ""questionContent"": ""Câu hỏi..."",
                    ""imageUrl"": ""/path/to/image.jpg"",
                    ""createdAt"": ""2024-01-01T00:00:00Z"",
                    ""updatedAt"": ""2024-01-01T00:00:00Z""
                }
                ]
            }
            }
            ```
            - **Lưu ý**: Response này KHÔNG bao gồm đáp án (`CorrectAnswer`), điểm (`Mark`), hoặc câu trả lời của user.

            **3. Trường hợp: Mock test đã hoàn thành (Completed - Status = 1)**
            - Response type: `MockTestResultReviewResponse`
            - Schema:
            ```json
            {
            ""message"": ""Lấy kết quả mock test thành công"",
            ""statusCode"": 
                ""mockTestId"": 1,
                ""mockTestTitle"": ""Mock Test Title"",
                ""mockTestDescription"": ""Description"",
                ""createdAt""200,
            ""data"": {
                ""mockTestResultId"": 1,
                ""score"": 8.5,:
                ""dateTaken"": ""2024-01-01T00:00:00Z"",
                ""mockTest"": {
                ""mockTestId"": 1,
                ""mockTestTitle"": ""Mock Test Title"",
                ""mockTestDescription"": ""Description"",
                ""createdAt"": ""2024-01-01T00:00:00Z"",
                ""updatedAt"": ""2024-01-01T00:00:00Z"",
                ""createdBy"": ""userId"",
                ""questions"" [
                    {
                    ""questionId"": 1,
                    ""questionContent"": ""Câu hỏi..."",
                    ""correctAnswer"": ""Đáp án đúng"",
                    ""imageUrl"": ""/path/to/image.jpg"",
                    ""mark"": 1.0,
                    ""partialMark"": ""Partial mark description"",
                    ""userAnswer"": {
                        ""mockTestUserAnswerId"": 1,
                        ""userAnswer"": ""Câu trả lời của user"",
                        ""userMark"": 0.5
                    }
                    }
                ]
                }
            }
            }
            ```
            - **Lưu ý**: Response này BAO GỒM đầy đủ: đáp án đúng (`correctAnswer`), điểm (`mark`, `partialMark`), câu trả lời của user (`userAnswer`), và điểm user đạt được (`userMark`).")]
        public async Task<ActionResult<BaseResponse<object>>> GetMockTestForStudent([FromQuery] int id)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.GetMockTestByIdOrReviewAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("get-mocktest-results")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy danh sách kết quả các bài thi mock test đã làm", 
            Description ="Api dùng để lấy danh sách toàn bộ kết quả bài mock test đã làm của người dùng. " +
            "`MockTestId` để lọc theo danh sách bài mock test.")]
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
