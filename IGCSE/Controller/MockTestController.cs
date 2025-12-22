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
**3. Trường hợp: User có role manager**
```json
{
  ""message"": ""Lấy mocktest thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""mockTestId"": 25,
    ""mockTestTitle"": ""Mathematics 0580 Past Papers 2025 March"",
    ""mockTestDescription"": ""Đề thi tháng 3 môn Toán 0580"",
    ""createdAt"": ""2025-12-10T05:58:23"",
    ""questions"": [
      {
        ""questionId"": 0,
        ""questionContent"": ""Write the number twenty thousand in figures."",
        ""correctAnswer"": ""20 000"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Write 0.07 as a percentage"",
        ""correctAnswer"": ""7"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Write 0.07 as a fraction"",
        ""correctAnswer"": ""7/100"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Write 0.07 in standard form"",
        ""correctAnswer"": ""7x10-2"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Write down all the factors of 18."",
        ""correctAnswer"": ""1, 2, 3, 6, 9, 18 "",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Find the value of 10^3"",
        ""correctAnswer"": ""1000"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Find the value of squareroot of 9"",
        ""correctAnswer"": ""3"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Find the value of 70"",
        ""correctAnswer"": ""1"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Find the value of the reciprocal of 0.25 ."",
        ""correctAnswer"": ""4"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Work out  -6 x -3 + 7 x 2."",
        ""correctAnswer"": ""32"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Write these numbers in order, starting with the smallest: 34%, π, 1/3, 3, 3/10"",
        ""correctAnswer"": ""3/10, 1/3, 34%, 3, π"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""A film starts at 19 35. The film lasts for 70 minutes. Work out the time the film finishes."",
        ""correctAnswer"": ""20 45"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Work out: 1.95 x 2.04"",
        ""correctAnswer"": ""3.978"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Work out: 3978 / 204"",
        ""correctAnswer"": ""19.5"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Kat has a method for finding the difference between two square numbers.\r\nHer method is (the sum of a and b) x (the difference between a and b) .\r\nShe shows her method for 172 - 132 :  \r\n172 - 132 = (17 + 13) x (17 -13) = 30 x 4 = 120\r\nWork out 292 - 212 using Kat’s method."",
        ""correctAnswer"": ""(29 + 21) x (29 - 21) = 50 x 8"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Convert 8 litres into cm3"",
        ""correctAnswer"": ""8000"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""n is an odd number. Write down the values of n that satisfy 7 < n ≤ 5 ."",
        ""correctAnswer"": ""9, 11, 13, 15"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""Pip thinks -1 8 1 1x is the inequality represented on the number line. Complete the statement:\nPip’s inequality is not correct because ______"",
        ""correctAnswer"": ""Pip needed to use two ⩽ signs oe"",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""The height, h metres, of a building is 635m, correct to the nearest metre.\r\nComplete this statement about the value of h: _____ ≤ h < _____"",
        ""correctAnswer"": ""634.5 635.5 "",
        ""imageUrl"": """"
      },
      {
        ""questionId"": 0,
        ""questionContent"": ""A circle has radius 7cm. A square has side x cm. The circumference of the circle is the same length as the perimeter of the square. Find the value of x.\nGive your answer in terms of r."",
        ""correctAnswer"": ""7π/2 or 3.5π"",
        ""imageUrl"": """"
      }
    ]
  }
}
```
            - **Lưu ý**: Response này BAO GỒM đầy đủ: đáp án đúng (`correctAnswer`), điểm (`mark`, `partialMark`), câu trả lời của user (`userAnswer`), và điểm user đạt được (`userMark`).")]
        public async Task<ActionResult<BaseResponse<object>>> GetMockTestDetail([FromQuery] int id)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _mockTestService.GetMockTestByIdOrReviewAsync(id, userId, userRole);
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
