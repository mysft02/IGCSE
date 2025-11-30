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
            Summary = "Lấy danh sách Final Quiz theo id",
            Description = @"Api tự động trả về response khác nhau dựa trên trạng thái final quiz:
- Nếu chưa làm: Trả về quiz để làm (không có đáp án)
- Nếu đã làm: Trả về kết quả quiz (có đáp án, điểm số, và câu trả lời của user)

**Request:**
- Query parameter: `id` (int) - ID của quiz cần lấy

**1. Trường hợp: Final Quiz chưa được làm (chưa có kết quả)**
- Response type: `FinalQuizResponse`
- Schema:
```json
{
  ""message"": ""Lấy bài Final Quiz thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""id"": 78,
    ""title"": ""Quiz Title"",
    ""description"": ""Mô tả về quiz"",
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
- **Lưu ý**: Response này KHÔNG bao gồm đáp án (`CorrectAnswer`) - chỉ có câu hỏi và hình ảnh (nếu có)

**2. Trường hợp: Final Quiz đã được làm (có kết quả)**
- Response type: `FinalQuizResultReviewResponse`
- Schema khi **PASS** (isPassed = true):
```json
{
  ""message"": ""Lấy kết quả Final Quiz thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""quizResultId"": 1,
    ""score"": 8.0,
    ""isPassed"": true,
    ""dateTaken"": ""2024-01-15T10:30:00Z"",
    ""quiz"": {
      ""quizId"": 78,
      ""title"": ""Quiz Title"",
      ""description"": ""Mô tả về Final Quiz"",
      ""questions"": [
        {
          ""questionId"": 1,
          ""questionContent"": ""Câu hỏi 1?"",
          ""imageUrl"": ""https://example.com/images/question1.jpg"",
          ""correctAnswer"": ""Đáp án đúng"",
          ""userAnswer"": {
            ""quizUserAnswerId"": 1,
            ""userAnswer"": ""Câu trả lời của user""
          }
        }
      ]
    }
  }
}
```

- Schema khi **FAIL** (isPassed = false):
```json
{
  ""message"": ""Lấy kết quả Final Quiz thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""quizResultId"": 1,
    ""score"": 3.0,
    ""isPassed"": false,
    ""dateTaken"": ""2024-01-15T10:30:00Z"",
    ""quiz"": {
      ""quizId"": 78,
      ""title"": ""Quiz Title"",
      ""description"": ""Mô tả về Final Quiz"",
      ""questions"": [
        {
          ""questionId"": 1,
          ""questionContent"": ""Câu hỏi 1?"",
          ""imageUrl"": ""https://example.com/images/question1.jpg"",
          ""correctAnswer"": null,
          ""userAnswer"": {
            ""quizUserAnswerId"": 1,
            ""userAnswer"": ""Câu trả lời của user""
          }
        }
      ]
    }
  }
}
```
- **Lưu ý**: 
  - Nếu **PASS** (`isPassed = true`): Response BAO GỒM đáp án đúng (`correctAnswer`) - học sinh có thể xem lại đáp án
  - Nếu **FAIL** (`isPassed = false`): Response KHÔNG bao gồm đáp án đúng (`correctAnswer = null`) - học sinh cần làm lại quiz để xem đáp án
  - Luôn có: điểm số (`score`), trạng thái pass/fail (`isPassed`), câu trả lời của user (`userAnswer`), và thời gian làm (`dateTaken`)

**Response Schema - Trường hợp lỗi:**

1. **Final Quiz chưa được mở khóa (chưa hoàn thành tất cả lesson items):**
```json
{
  ""message"": ""Bạn chưa mở khoá bài Final Quiz này. Vui lòng hoàn thành bài học trước."",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Quiz không tồn tại:**
```json
{
  ""message"": ""Không tìm thấy bài Final Quiz này."",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **User chưa đăng nhập:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- Final Quiz chỉ được mở khóa khi user đã hoàn thành **TẤT CẢ** lesson items trong course đó
- Nếu đã làm Final Quiz, API sẽ tự động trả về kết quả với đầy đủ thông tin (đáp án, điểm số, câu trả lời của user)
- Nếu chưa làm, API trả về Final Quiz để làm (không có đáp án)
- Field `imageUrl` có thể là `null` nếu câu hỏi không có hình ảnh
- `userAnswer` có thể là `null` nếu user không trả lời câu hỏi đó")]
        public async Task<ActionResult<BaseResponse<object>>> GetFinalQuizById([FromQuery] int id)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

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
            Description = @"Api dùng để chấm bài final quiz của học sinh. Hệ thống sử dụng AI để đánh giá câu trả lời và hoàn thành khoá học nếu quiz đạt (điểm > 50%).

**Request Body Schema:**
```json
{
  ""finalQuizId"": ""1""
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

**Response Schema - Trường hợp thành công (PASS - điểm > 50%):**
```json
{
  ""message"": ""Final Quiz marked successfully"",
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

**Response Schema - Trường hợp thành công (FAIL - điểm ≤ 50%):**
```json
{
  ""message"": ""Final Quiz marked successfully"",
  ""statusCode"": 200,
  ""data"": [
    {
      ""question"": ""Câu hỏi 1?"",
      ""answer"": ""Câu trả lời của học sinh"",
      ""rightAnswer"": null,
      ""isCorrect"": false,
      ""comment"": ""Câu trả lời chưa chính xác, cần xem lại kiến thức.""
    },
    {
      ""question"": ""Câu hỏi 2?"",
      ""answer"": ""Câu trả lời sai"",
      ""rightAnswer"": null,
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

2. **Đánh giá kết quả:**
   - Pass: Điểm > 50% tổng số câu hỏi
   - Fail: Điểm ≤ 50% tổng số câu hỏi

3. **Tự động mở khóa:**
   - Nếu quiz đạt (Pass), hệ thống tự động mở khóa lesson tiếp theo
   - Kết quả quiz được lưu vào database với `Score` và `IsPassed`

4. **Lưu câu trả lời:**
   - Tất cả câu trả lời của học sinh được lưu vào `FinalQuizUserAnswer` table

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- Câu hỏi không tồn tại sẽ bị bỏ qua (không ảnh hưởng đến kết quả)
- Nhận xét từ AI có độ dài < 100 ký tự
- So sánh đáp án không phân biệt hoa thường và bỏ qua khoảng trắng
- **Quan trọng**: 
  - Nếu quiz **PASS** (điểm > 50%): Response BAO GỒM `rightAnswer` (đáp án đúng)
  - Nếu quiz **FAIL** (điểm ≤ 50%): Response KHÔNG bao gồm `rightAnswer` (`rightAnswer = null`) - học sinh cần làm lại để xem đáp án
- Nếu có lỗi khi unlock lesson tiếp theo, lỗi sẽ được log nhưng không làm fail request")]
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
