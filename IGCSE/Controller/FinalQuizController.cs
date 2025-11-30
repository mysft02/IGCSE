using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Common.Utils;
using BusinessObject.DTOs.Request.FinalQuizzes;
using BusinessObject.DTOs.Response.FinalQuizzes;

namespace IGCSE.Controller
{
    [Route("api/final-quiz")]
    [ApiController]
    public class FinalQuizController : ControllerBase
    {
        private readonly FinalQuizService _finalQuizService;

        public FinalQuizController(FinalQuizService finalQuizService)
        {
            _finalQuizService = finalQuizService;
        }

        [HttpGet("get-final-quiz-by-id")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy thông tin Final Quiz theo id", 
            Description = @"Api dùng để lấy thông tin chi tiết của Final Quiz (bài thi cuối khóa) để học sinh thực hiện bài thi.

**Request:**
- Query parameter: `id` (int, required) - ID của Final Quiz cần lấy

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Final quiz retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""id"": 1,
    ""title"": ""Final Quiz Title"",
    ""description"": ""Mô tả về final quiz"",
    ""questions"": [
      {
        ""questionId"": 1,
        ""questionContent"": ""Câu hỏi 1?"",
        ""imageUrl"": ""https://example.com/images/question1.jpg""
      },
      {
        ""questionId"": 2,
        ""questionContent"": ""Câu hỏi 2?"",
        ""imageUrl"": null
      }
    ]
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

2. **Final Quiz không tồn tại:**
```json
{
  ""message"": ""Final quiz not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Response KHÔNG bao gồm đáp án (`CorrectAnswer`) - chỉ có câu hỏi và hình ảnh (nếu có)
- Field `imageUrl` có thể là `null` nếu câu hỏi không có hình ảnh")]
        public async Task<ActionResult<BaseResponse<FinalQuizResponse>>> GetFinalQuizById([FromQuery] int id)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _finalQuizService.GetFinalQuizByIdAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("mark-final-quiz")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Chấm bài Final Quiz", 
            Description = @"Api dùng để chấm bài Final Quiz (bài thi cuối khóa) của học sinh. Hệ thống sử dụng AI để đánh giá câu trả lời.

**Request:**
- Body:
```json
{
  ""finalQuizID"": 1,
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
  - `finalQuizID` (int, required) - ID của Final Quiz
  - `userAnswers` (array, required) - Danh sách câu trả lời của học sinh
    - `questionId` (int, required) - ID của câu hỏi
    - `answer` (string, required) - Câu trả lời của học sinh

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Final quiz marked successfully"",
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

**Response Schema - Trường hợp lỗi:**

1. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

2. **Final Quiz không tồn tại:**
```json
{
  ""message"": ""Final quiz not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Học sinh chưa hoàn thành khóa học:**
```json
{
  ""message"": ""Student has not completed the course"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Logic xử lý:**
1. **Chấm điểm:**
   - Sử dụng AI (GPT-4o-mini) để đánh giá câu trả lời
   - So sánh đáp án của học sinh với đáp án đúng
   - Tạo nhận xét cho từng câu

2. **Lưu kết quả:**
   - Kết quả được lưu vào `FinalQuizResult` table
   - Tất cả câu trả lời được lưu vào `FinalQuizUserAnswer` table
   - Điểm số và trạng thái pass/fail được tính toán và lưu

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Học sinh phải hoàn thành 100% khóa học mới có thể làm Final Quiz
- Nhận xét từ AI có độ dài < 100 ký tự
- Response bao gồm đáp án đúng (`rightAnswer`) để học sinh có thể xem lại")]
        public async Task<ActionResult<BaseResponse<List<FinalQuizMarkResponse>>>> MarkFinalQuiz([FromBody] FinalQuizMarkRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _finalQuizService.MarkFinalQuizAsync(request, userId);
            return Ok(result);
        }
    }
}
