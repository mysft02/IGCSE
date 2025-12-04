using BusinessObject.DTOs.Request.MockTest;
using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.DTOs.Response.Quizzes;
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
            Summary = "Lấy danh sách mock test",
            Description = @"
Lấy danh sách mock test với các trạng thái:

- `1` là `Completed` (đã hoàn thành bài thi trước đó);

- `2` là `Open` (đã mua gói nhưng chưa hoàn thành bài thi trước đó);

- `3` là `Locked` (chưa mua gói bài thi)

**Response type**: `PaginatedResponse<MockTestResponse>`

**Schema:**

```json
{
    ""message"": ""Lấy mock test thành công"",
    ""statusCode"": 200,
    ""data"": {
        ""items"": [
            {
                ""mockTestId"": 1,
                ""mockTestTitle"": ""Bài thi 1"",
                ""mockTestDescription"": ""Đại số cơ bản"",
                ""createdAt"": ""2025-10-28T12:41:41"",
                ""updatedAt"": ""2025-10-28T12:41:41"",
                ""createdBy"": ""e12b76d9-f8d2-478c-b83b-063c7f5df0f6"",
                ""status"": 3
            }
        ],
        ""totalCount"": 7,
        ""page"": 0,
        ""size"": 10,
        ""totalPages"": 1,
        ""hasNextPage"": false,
        ""hasPreviousPage"": false,
        ""currentPage"": 1
    }
}"
)]
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
            Description = @"Api dùng để chấm bài mocktest của học sinh. Hệ thống sử dụng AI để đánh giá câu trả lời.

**Request Body Schema:**
```json
{
  ""mockTestId"": ""1"",
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

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""MockTest marked successfully"",
  ""statusCode"": 200,
  ""data"": [
    {
      ""question"": ""Câu hỏi 1?"",
      ""answer"": ""Câu trả lời của học sinh"",
      ""rightAnswer"": ""Đáp án đúng"",
      ""isCorrect"": true,
      ""comment"": ""Câu trả lời chính xác và đầy đủ.""
    },
    {
      ""question"": ""Câu hỏi 2?"",
      ""answer"": ""Câu trả lời sai"",
      ""rightAnswer"": ""Đáp án đúng"",
      ""isCorrect"": false,
      ""comment"": ""Câu trả lời chưa chính xác, cần xem lại kiến thức.""
    }
  ]
}
```

**Response Schema - Trường hợp không có câu trả lời:**
```json
{
  ""message"": ""No answers to mark"",
  ""statusCode"": 200,
  ""data"": []
}
```

**Response Schema - Trường hợp lỗi:**

1. **OpenAI trả về rỗng:**
```json
{
  ""message"": ""OpenAI trả về rỗng (outputText empty)"",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **User chưa đăng nhập:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Logic xử lý:**
1. **Chấm điểm:**
   - Mỗi câu trả lời đúng = 1 điểm
   - So sánh đáp án của học sinh với đáp án đúng (case-insensitive, bỏ qua khoảng trắng)
   - Sử dụng AI (GPT-4o-mini) để tạo nhận xét cho từng câu

2. **Lưu câu trả lời:**
   - Tất cả câu trả lời của học sinh được lưu vào `MockTestUserAnswer` table

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- Câu hỏi không tồn tại sẽ bị bỏ qua (không ảnh hưởng đến kết quả)
- Nhận xét từ AI có độ dài < 100 ký tự
- So sánh đáp án không phân biệt hoa thường và bỏ qua khoảng trắng")]
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

        [HttpPost("create-mocktest")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = "Tạo bài mocktest",
            Description = @"Api dùng để tạo bài mocktest cho teacher.

**Request Query:**
- ""MockTestTitle"": Tiêu đề bài mocktest
- ""MockTestDescription"": Mô tả bài mocktest
- ""ExcelFile"": File Excel bài mocktest

**Response Schema:**
1. Trường hợp tạo thành công:
```json
{
  ""message"": ""Tạo bài mock test thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""mockTestId"": 20,
    ""mockTestTitle"": ""testfinal"",
    ""mockTestDescription"": ""testfinal"",
    ""createdAt"": ""2025-12-04T01:45:31.1654015Z"",
    ""updatedAt"": ""2025-12-04T01:45:31.1654516Z"",
    ""mockTestQuestions"": [
      {
        ""mockTestQuestionId"": 99,
        ""mockTestId"": 20,
        ""questionContent"": ""1+1=?"",
        ""correctAnswer"": ""2"",
        ""partialMark"": ""if 3 is wrong"",
        ""mark"": 2
      }
    ]
  }
}
```")]
        public async Task<ActionResult<BaseResponse<MockTestCreateResponse>>> CreateQuiz([FromQuery] MockTestCreateRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.CreateMockTestAsync(request, userId);
            return Ok(result);
        }
    }
}
