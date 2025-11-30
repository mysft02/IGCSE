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
        [SwaggerOperation(
            Summary = "Lấy danh sách mock test (có paging và filter)", 
            Description = @"Api dùng để lấy danh sách tất cả mock test với phân trang và bộ lọc. Hệ thống tự động xác định trạng thái của từng mock test dựa trên việc user đã mua gói và đã làm bài thi.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `MockTestQueryRequest`

**Trạng thái Mock Test:**
- `1` - `Completed`: Đã hoàn thành bài thi trước đó
- `2` - `Open`: Đã mua gói nhưng chưa hoàn thành bài thi trước đó
- `3` - `Locked`: Chưa mua gói bài thi

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy mock test thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""mockTestId"": 1,
        ""mockTestTitle"": ""Mock Test Title"",
        ""mockTestDescription"": ""Description"",
        ""status"": 2,
        ""createdAt"": ""2024-01-01T00:00:00Z"",
        ""updatedAt"": ""2024-01-01T00:00:00Z""
      }
    ],
    ""totalCount"": 50,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 5
  }
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Error message"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Nếu user đã đăng nhập, `userID` được lấy tự động từ JWT token để xác định trạng thái
- Nếu user chưa đăng nhập, tất cả mock test sẽ có status = `3` (Locked)")]
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
        [SwaggerOperation(
            Summary = "Lấy danh sách kết quả các bài thi mock test đã làm", 
            Description = @"Api dùng để lấy danh sách toàn bộ kết quả bài mock test đã làm của người dùng với phân trang và bộ lọc.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - `MockTestId` (int, optional) - Lọc theo ID bài mock test
  - Các query parameters khác tùy theo `MockTestResultQueryRequest`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy kết quả mock test thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""mockTestResultId"": 1,
        ""mockTestId"": 1,
        ""mockTestTitle"": ""Mock Test Title"",
        ""score"": 8.5,
        ""dateTaken"": ""2024-01-15T10:30:00Z"",
        ""isPassed"": true
      }
    ],
    ""totalCount"": 20,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 2
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Chỉ trả về kết quả của chính user đó
- Có thể lọc theo `MockTestId` để xem kết quả của một bài mock test cụ thể")]
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
        [SwaggerOperation(
            Summary = "Chấm bài mock test", 
            Description = @"Api dùng để chấm bài mock test của học sinh. Hệ thống sử dụng AI để đánh giá câu trả lời và tính điểm.

**Request:**
- Body:
```json
{
  ""mockTestId"": 1,
  ""userAnswers"": [
    {
      ""questionId"": 1,
      ""answer"": ""Câu trả lời của học sinh""
    },
    {
      ""questionId"": 2,
      ""answer"": ""Câu trả lời khác""
    }
  ]
}
```
  - `mockTestId` (int, required) - ID của mock test
  - `userAnswers` (array, required) - Danh sách câu trả lời của học sinh
    - `questionId` (int, required) - ID của câu hỏi
    - `answer` (string, required) - Câu trả lời của học sinh

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Mock test marked successfully"",
  ""statusCode"": 200,
  ""data"": [
    {
      ""question"": ""Câu hỏi 1?"",
      ""answer"": ""Câu trả lời của học sinh"",
      ""rightAnswer"": ""Đáp án đúng"",
      ""isCorrect"": true,
      ""comment"": ""Câu trả lời chính xác và đầy đủ."",
      ""mark"": 1.0,
      ""userMark"": 1.0
    },
    {
      ""question"": ""Câu hỏi 2?"",
      ""answer"": ""Câu trả lời sai"",
      ""rightAnswer"": ""Đáp án đúng"",
      ""isCorrect"": false,
      ""comment"": ""Câu trả lời chưa chính xác."",
      ""mark"": 1.0,
      ""userMark"": 0.0
    }
  ]
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **Mock test không tồn tại:**
```json
{
  ""message"": ""Mock test not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Chưa mua gói mock test:**
```json
{
  ""message"": ""You have not purchased this mock test package"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Logic xử lý:**
1. **Chấm điểm:**
   - Sử dụng AI để đánh giá câu trả lời
   - Tính điểm dựa trên `mark` và `partialMark` của từng câu hỏi
   - Tạo nhận xét cho từng câu

2. **Lưu kết quả:**
   - Kết quả được lưu vào `MockTestResult` table
   - Tất cả câu trả lời được lưu vào `MockTestUserAnswer` table
   - Điểm số và trạng thái pass/fail được tính toán và lưu

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- User phải mua gói mock test trước khi có thể làm bài
- Nhận xét từ AI có độ dài < 100 ký tự
- Response bao gồm đáp án đúng (`rightAnswer`), điểm (`mark`, `userMark`) và nhận xét (`comment`)")]
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
