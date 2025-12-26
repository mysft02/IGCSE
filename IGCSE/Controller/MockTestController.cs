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
using System.Security.Claims;

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
        [Authorize]
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
        public async Task<ActionResult<BaseResponse<PaginatedResponse<MockTestQueryResponse>>>> GetAllMockTests([FromQuery] MockTestQueryRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            request.userID = userId;

            var result = await _mockTestService.GetAllMockTestAsync(request, userRole);
            return Ok(result);
        }

        [HttpGet("get-mocktest-result")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy kết quả bài mock test đã làm",
            Description = @"Api dùng để lấy kết quả chi tiết của bài mock test đã hoàn thành. Chỉ trả về khi user đã hoàn thành bài thi.

**Request:**
- Query parameter: `id` (int) - ID của mock test

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy kết quả mock test thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""mockTestResultId"": 1,
    ""score"": 8.5,
    ""dateTaken"": ""2024-01-01T00:00:00Z"",
    ""mockTest"": {
      ""mockTestId"": 1,
      ""mockTestTitle"": ""Mock Test Title"",
      ""mockTestDescription"": ""Description"",
      ""createdAt"": ""2024-01-01T00:00:00Z"",
      ""updatedAt"": ""2024-01-01T00:00:00Z"",
      ""createdBy"": ""userId"",
      ""questions"": [
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

**Response Schema - Trường hợp lỗi:**

1. **Chưa hoàn thành bài thi:**
```json
{
  ""message"": ""Bạn chưa hoàn thành bài thi này."",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **Không tìm thấy kết quả:**
```json
{
  ""message"": ""Không tìm thấy kết quả bài thi."",
  ""statusCode"": 500,
  ""data"": null
}
```

3. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- Chỉ trả về kết quả nếu đã hoàn thành bài thi (status = Completed)
- Response bao gồm đầy đủ: đáp án đúng, điểm, câu trả lời của user, và điểm user đạt được
- User ID được lấy tự động từ JWT token")]
        public async Task<ActionResult<BaseResponse<MockTestResultReviewResponse>>> GetMockTestResult([FromQuery] int id)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.GetMockTestResultAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("get-mocktest-by-id")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy mock test để làm bài hoặc xem với đáp án",
            Description = @"Api dùng để lấy bài mock test. Response khác nhau tùy theo role của user:

**1. Student role:**
- Lấy bài thi để làm (chưa hoàn thành)
- Response KHÔNG bao gồm đáp án, điểm
- Yêu cầu đã đăng ký gói thi thử

**2. Manager role:**
- Lấy bài thi với đáp án đầy đủ
- Response BAO GỒM đáp án, điểm, partial mark

**Request:**
- Query parameter: `id` (int) - ID của mock test

**Response Schema - Student (chưa hoàn thành):**
```json
{
  ""message"": ""Lấy mock test thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""mockTestId"": 1,
    ""mockTestTitle"": ""Mock Test Title"",
    ""mockTestDescription"": ""Description"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
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

**Response Schema - Manager (có đáp án):**
```json
{
  ""message"": ""Lấy mock test thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""mockTestId"": 1,
    ""mockTestTitle"": ""Mock Test Title"",
    ""mockTestDescription"": ""Description"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""questions"": [
      {
        ""questionId"": 1,
        ""questionContent"": ""Câu hỏi..."",
        ""correctAnswer"": ""Đáp án đúng"",
        ""imageUrl"": ""/path/to/image.jpg"",
        ""mark"": 1.0,
        ""partialMark"": ""Partial mark description""
      }
    ]
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không có quyền (Parent/Teacher):**
```json
{
  ""message"": ""Bạn không có quyền coi bài thi thử này."",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **Chưa đăng ký gói thi thử (Student):**
```json
{
  ""message"": ""Bạn chưa đăng kí gói thi thử."",
  ""statusCode"": 500,
  ""data"": null
}
```

3. **Không tìm thấy bài thi:**
```json
{
  ""message"": ""Bài thi thử không tìm thấy"",
  ""statusCode"": 500,
  ""data"": null
}
```

4. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- Parent và Teacher không có quyền sử dụng API này
- Student: Response KHÔNG bao gồm đáp án (`CorrectAnswer`), điểm (`Mark`)
- Manager: Response BAO GỒM đầy đủ đáp án, điểm, partial mark
- User ID và Role được lấy tự động từ JWT token")]
        public async Task<ActionResult<BaseResponse<object>>> GetMockTestById([FromQuery] int id)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }
            if(userRole == "Parent" || userRole == "Teacher")
            {
                throw new Exception("Bạn không có quyền coi bài thi thử này.");
            }

            var result = await _mockTestService.GetMockTestByIdAsync(id, userId, userRole);
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
        [Authorize]
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
        public async Task<ActionResult<BaseResponse<MockTestCreateResponse>>> CreateMockTest([FromQuery] MockTestCreateRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.CreateMockTestAsync(request, userId);
            return Ok(result);
        }

        [HttpDelete("delete-mocktest")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = "Xóa mock test theo ID",
            Description = @"Api dùng để xóa mock test. Chỉ người tạo mock test mới có quyền xóa.

**Request:**
- Query parameter: `id` (int) - ID của mock test cần xóa

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Xóa mock test thành công"",
  ""statusCode"": 200,
  ""data"": ""Mock test đã được xóa thành công.""
}
```

**Response Schema - Trường hợp lỗi:**

1. **Mock test không tồn tại:**
```json
{
  ""message"": ""Mock test không tồn tại"",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **Không có quyền xóa:**
```json
{
  ""message"": ""Bạn không có quyền xóa mock test này."",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập và role Teacher hoặc Manager
- Chỉ người tạo mock test mới có quyền xóa
- Hành động xóa là vĩnh viễn và không thể hoàn tác
- Tất cả dữ liệu liên quan (câu hỏi, kết quả thi) sẽ bị xóa theo cascade")]
        public async Task<ActionResult<BaseResponse<string>>> DeleteMockTest([FromQuery] int id)
        {
            var result = await _mockTestService.DeleteMockTestAsync(id);
            return Ok(result);
        }
    }
}
