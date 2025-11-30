using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.ParentStudentLink;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using Common.Constants;
using System.Security.Claims;


namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly ModuleService _moduleService;
        private readonly MediaService _mediaService;
        private readonly IWebHostEnvironment _environment;
        private readonly PaymentService _paymentService;
        private readonly CourseFeedbackService _courseFeedbackService;

        public CourseController(
            CourseService courseService,
            ModuleService moduleService,
            MediaService mediaService,
            IWebHostEnvironment environment,
            PaymentService paymentService,
            CourseFeedbackService courseFeedbackService)
        {
            _mediaService = mediaService;
            _environment = environment;
            _moduleService = moduleService;
            _courseService = courseService;
            _paymentService = paymentService;
            _courseFeedbackService = courseFeedbackService;
        }

        [HttpGet("all")]
        [SwaggerOperation(
            Summary = "Lấy danh sách các khóa học (có paging và filter)", 
            Description = @"Api dùng để lấy danh sách khóa học với phân trang và bộ lọc. Hệ thống tự động áp dụng filter theo role của user.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 1) - Số trang
  - `PageSize` (int, mặc định: 10) - Số lượng item mỗi trang
  - `SearchByName` (string, optional) - Tìm kiếm theo tên khóa học
  - `CouseId` (int, optional) - Lọc theo ID khóa học
  - `Status` (string, optional) - Lọc theo trạng thái: `Open` (đã duyệt), `Pending` (chưa duyệt), `Rejected` (bị từ chối)

**Logic tự động:**
- Nếu user đã đăng nhập với role `Student` hoặc `Parent`: tự động filter chỉ lấy khóa học có status = `Open`
- Nếu user chưa đăng nhập hoặc role khác: có thể lọc theo bất kỳ status nào

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Courses retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""courseId"": 1,
        ""name"": ""Tên khóa học"",
        ""description"": ""Mô tả khóa học"",
        ""status"": ""Open"",
        ""price"": 1000000,
        ""imageUrl"": ""/path/to/image.jpg"",
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
- Kết quả được sắp xếp mặc định theo thời gian tạo mới nhất")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseResponse>>>> GetAllCourses([FromQuery] CourseListQuery query)
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                    if (roles.Contains("Parent") || roles.Contains("Student"))
                    {
                        query.Status = "Open";
                    }
                }

                var result = await _courseService.GetCoursesPagedAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("{courseId}/feedbacks")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(
            Summary = "Tạo feedback cho khóa học (Student)", 
            Description = @"Api dùng để học sinh tạo feedback (đánh giá và nhận xét) cho khóa học. Chỉ học sinh đã hoàn thành khóa học mới có thể gửi feedback.

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học
- Body:
```json
{
  ""rating"": 5,
  ""comment"": ""Khóa học rất hay và bổ ích""
}
```
  - `rating` (int, required) - Điểm đánh giá từ 1 đến 5
  - `comment` (string, optional, max 2000 ký tự) - Nhận xét về khóa học

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Tạo feedback thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""courseFeedbackId"": 1,
    ""courseId"": 34,
    ""studentId"": ""user-id-123"",
    ""studentName"": ""Nguyễn Văn A"",
    ""rating"": 5,
    ""comment"": ""Khóa học rất hay và bổ ích"",
    ""createdAt"": ""2024-01-15T10:30:00Z"",
    ""updatedAt"": ""2024-01-15T10:30:00Z""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Khóa học không tồn tại:**
```json
{
  ""message"": ""Lỗi khi gửi feedback: Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Học sinh chưa đăng ký khóa học:**
```json
{
  ""message"": ""Lỗi khi gửi feedback: Bạn cần đăng ký khóa học này trước khi gửi feedback"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Học sinh chưa hoàn thành khóa học:**
```json
{
  ""message"": ""Lỗi khi gửi feedback: Chỉ học viên đã hoàn thành khóa học mới có thể gửi feedback"",
  ""statusCode"": 400,
  ""data"": null
}
```

4. **Đã gửi feedback trước đó:**
```json
{
  ""message"": ""Lỗi khi gửi feedback: Bạn đã gửi feedback cho khóa học này"",
  ""statusCode"": 400,
  ""data"": null
}
```

5. **Rating không hợp lệ:**
```json
{
  ""message"": ""Lỗi khi gửi feedback: Rating must be between 1 and 5"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Student role mới có quyền sử dụng API này
- Mỗi học sinh chỉ có thể gửi một feedback cho mỗi khóa học
- Học sinh phải hoàn thành 100% khóa học (tất cả lessons) mới có thể gửi feedback")]
        public async Task<ActionResult<BaseResponse<CourseFeedbackResponse>>> CreateFeedback(int courseId, [FromBody] CourseFeedbackRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseFeedbackService.CreateFeedbackAsync(userId, courseId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi gửi feedback: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("{courseId}/feedbacks")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Lấy danh sách feedback của khóa học", 
            Description = @"Api dùng để lấy danh sách feedback của một khóa học.

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học
- Query parameters:
  - `Page` (int, mặc định: 1) - Số trang
  - `PageSize` (int, mặc định: 10) - Số lượng item mỗi trang
  - `Rating` (int, optional) - Lọc theo rating (1-5)
  - `SearchByStudentName` (string, optional) - Tìm kiếm theo tên học viên
  - `SortBy` (string, mặc định: ""date"") - Sắp xếp theo: ""date"" (ngày tạo) hoặc ""rating"" (điểm đánh giá)
  - `SortOrder` (string, mặc định: ""desc"") - Thứ tự: ""asc"" (tăng dần) hoặc ""desc"" (giảm dần)

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy danh sách feedback thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""courseFeedbackId"": 1,
        ""courseId"": 34,
        ""studentId"": ""user-id-123"",
        ""studentName"": ""Nguyễn Văn A"",
        ""rating"": 5,
        ""comment"": ""Khóa học rất hay"",
        ""createdAt"": ""2024-01-15T10:30:00Z"",
        ""updatedAt"": ""2024-01-15T10:30:00Z""
      }
    ],
    ""totalCount"": 25,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 3
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Khóa học không tồn tại:**
```json
{
  ""message"": ""Lỗi khi lấy danh sách feedback: Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Mặc định sắp xếp theo ngày tạo mới nhất (desc)
- Có thể kết hợp nhiều filter cùng lúc")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseFeedbackResponse>>>> GetCourseFeedbacks(int courseId, [FromQuery] CourseFeedbackQueryRequest request)
        {
            try
            {
                var result = await _courseFeedbackService.GetCourseFeedbacksPagedAsync(courseId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy danh sách feedback: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("{courseId}/feedbacks/summary")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Lấy thống kê feedback của khóa học", 
            Description = @"Api dùng để lấy thống kê tổng quan về feedback của một khóa học (điểm trung bình và tổng số feedback).

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy thống kê feedback thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""averageRating"": 4.5,
    ""totalFeedback"": 25
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Khóa học không tồn tại:**
```json
{
  ""message"": ""Lỗi khi lấy thống kê feedback: Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- `averageRating` được làm tròn đến 2 chữ số thập phân
- Nếu chưa có feedback nào, `averageRating` = 0 và `totalFeedback` = 0")]
        public async Task<ActionResult<BaseResponse<object>>> GetCourseFeedbackSummary(int courseId)
        {
            try
            {
                var result = await _courseFeedbackService.GetCourseFeedbackSummaryAsync(courseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy thống kê feedback: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("complete-lesson-item")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(
            Summary = "Đánh dấu hoàn thành lesson item (Student)", 
            Description = @"Api dùng để đánh dấu đã hoàn thành lesson item khi học sinh đã học xong. API này có logic tự động mở khóa lesson tiếp theo khi hoàn thành tất cả lesson items trong lesson hiện tại.

**Request:**
- Query parameter: `lessonItemId` (int) - ID của lesson item cần đánh dấu hoàn thành

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lesson item completed successfully"",
  ""statusCode"": 200,
  ""data"": true
}
```

**Response Schema - Trường hợp đã hoàn thành trước đó (Idempotent):**
```json
{
  ""message"": ""Lesson item already completed"",
  ""statusCode"": 200,
  ""data"": true
}
```

**Response Schema - Trường hợp lỗi:**

1. **Lesson item không tồn tại:**
```json
{
  ""message"": ""Lỗi khi hoàn thành thành phần của bài học: Lesson item not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Student chưa enroll vào course/lesson:**
```json
{
  ""message"": ""Lỗi khi hoàn thành thành phần của bài học: Student is not enrolled in this course or lesson not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Lesson chưa được mở khóa:**
```json
{
  ""message"": ""Lỗi khi hoàn thành thành phần của bài học: This lesson is locked. Please complete previous lessons first."",
  ""statusCode"": 400,
  ""data"": null
}
```

**Logic tự động:**
- Khi hoàn thành một lesson item, hệ thống sẽ kiểm tra xem tất cả lesson items trong lesson đó đã hoàn thành chưa
- Nếu tất cả lesson items đã hoàn thành:
  - Lesson hiện tại được đánh dấu hoàn thành
  - Lesson tiếp theo trong cùng section sẽ được tự động mở khóa (`IsUnlocked = true`)
  - Nếu đã hết lesson trong section hiện tại, lesson đầu tiên của section tiếp theo sẽ được mở khóa

**Lưu ý:**
- Chỉ Student role mới có quyền sử dụng API này
- API có tính idempotent: gọi nhiều lần với cùng lessonItemId sẽ không tạo duplicate record
- Cần hoàn thành lesson trước đó trước khi có thể hoàn thành lesson tiếp theo")]
        public async Task<ActionResult<BaseResponse<bool>>> CompleteLessonItem([FromQuery] int lessonItemId)
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.CompleteLessonItemAsync(userId, lessonItemId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi hoàn thành thành phần của bài học: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("my-registrations")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(
            Summary = "Lấy danh sách khóa học đã đăng ký của chính mình (Student)", 
            Description = @"Api dùng để học sinh xem danh sách các khóa học đã đăng ký (enroll) của mình với phân trang.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `CourseRegistrationQueryRequest`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy danh sách khóa học đã đăng ký thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""courseId"": 34,
        ""courseName"": ""Tên khóa học"",
        ""enrolledAt"": ""2024-01-01T00:00:00Z"",
        ""overallProgress"": 65.5
      }
    ],
    ""totalCount"": 10,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 1
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không xác định được tài khoản."",
  ""statusCode"": 401,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Student role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Kết quả chỉ trả về các khóa học mà học sinh đã enroll")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseRegistrationResponse>>>> GetMyRegistrations([FromQuery] CourseRegistrationQueryRequest request)
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.GetStudentRegistrationsAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy danh sách khóa học đã đăng ký: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("my-create-course")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Lấy tất cả khóa học do teacher đã tạo (Teacher)", 
            Description = @"Api dùng để giáo viên xem danh sách các khóa học do chính mình tạo với phân trang và filter.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `TeacherCourseQueryRequest`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy danh sách khóa học thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""courseId"": 34,
        ""name"": ""Tên khóa học"",
        ""description"": ""Mô tả khóa học"",
        ""status"": ""Open"",
        ""price"": 1000000,
        ""imageUrl"": ""/path/to/image.jpg"",
        ""createdAt"": ""2024-01-01T00:00:00Z"",
        ""updatedAt"": ""2024-01-01T00:00:00Z""
      }
    ],
    ""totalCount"": 15,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 2
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không xác định được tài khoản."",
  ""statusCode"": 401,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Teacher ID được lấy tự động từ JWT token
- Kết quả chỉ trả về các khóa học do chính teacher đó tạo")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseResponse>>>> GetMyCreatedCourses([FromQuery] TeacherCourseQueryRequest request)
        {
            var user = HttpContext.User;
            var teacherId = user.FindFirst("AccountID")?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
            }

            request.userID = teacherId;

            var result = await _courseService.GetTeacherCoursesAsync(request);
            return Ok(result);
        }

        [HttpGet("get-all-similar-courses")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy danh sách các khóa học tương tự", 
            Description = @"Api dùng để lấy danh sách các khóa học tương tự với khóa học hiện tại dựa trên điểm số final quiz của user.

**Request:**
- Query parameter: `CourseId` (int, required) - ID của khóa học hiện tại

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Courses retrieved successfully"",
  ""statusCode"": 200,
  ""data"": [
    {
      ""courseId"": 35,
      ""name"": ""Khóa học tương tự"",
      ""description"": ""Mô tả khóa học"",
      ""status"": ""Open"",
      ""price"": 1000000,
      ""imageUrl"": ""/path/to/image.jpg"",
      ""createdAt"": ""2024-01-01T00:00:00Z"",
      ""updatedAt"": ""2024-01-01T00:00:00Z""
    }
  ]
}
```

**Response Schema - Trường hợp lỗi:**

1. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Dữ liệu không hợp lệ"",
  ""statusCode"": 400,
  ""data"": ""Error details""
}
```

**Logic:**
- Hệ thống sử dụng điểm số final quiz của user để tính toán độ tương đồng
- Nếu user chưa làm final quiz, score mặc định = 1
- Các khóa học được sắp xếp theo độ tương đồng giảm dần

**Lưu ý:**
- Cần đăng nhập (Authorize) để sử dụng API này
- User ID được lấy tự động từ JWT token")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetAllSimilarCourses([FromQuery] SimilarCourseRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            var result = await _courseService.GetAllSimilarCoursesAsync(request.CourseId, userId);
            return Ok(result);
        }

        [HttpGet("{courseId}")]
        [SwaggerOperation(
            Summary = "Lấy tất cả thông tin chi tiết của khóa học", 
            Description = @"Api tự động trả về response khác nhau dựa trên trạng thái đăng nhập và enrollment của user:

**1. Trường hợp: User chưa đăng nhập hoặc không phải Student role**
- Response type: `CourseDetailResponse`
- `IsEnrolled`: `false`
- `OverallProgress`: `null` (không có thông tin tiến trình)
- Các lesson: `IsUnlocked = false`, `IsCompleted = false`
- Các lesson items: `IsCompleted = false`, `CompletedAt = null`
- Schema:
```json
{
  ""message"": ""Course detail retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""name"": ""Tên khóa học"",
    ""description"": ""Mô tả khóa học"",
    ""status"": ""Open"",
    ""price"": 1000000,
    ""imageUrl"": ""/path/to/image.jpg"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-01T00:00:00Z"",
    ""isEnrolled"": false,
    ""overallProgress"": null,
    ""sections"": [
      {
        ""courseId"": 34,
        ""courseSectionId"": 1,
        ""name"": ""Section 1"",
        ""description"": ""Mô tả section"",
        ""order"": 1,
        ""isActive"": true,
        ""lessons"": [
          {
            ""lessonId"": 1,
            ""courseSectionId"": 1,
            ""name"": ""Lesson 1"",
            ""description"": ""Mô tả lesson"",
            ""order"": 1,
            ""isActive"": true,
            ""isUnlocked"": false,
            ""isCompleted"": false,
            ""lessonItems"": [
              {
                ""lessonItemId"": 1,
                ""name"": ""Video 1"",
                ""order"": 1,
                ""isCompleted"": false,
                ""completedAt"": null
              }
            ],
            ""quiz"": {
              ""quizId"": 1,
              ""quizTitle"": ""Quiz Title"",
              ""quizDescription"": ""Quiz Description""
            }
          }
        ]
      }
    ]
  }
}
```

**2. Trường hợp: User đã đăng nhập (Student role) nhưng chưa enroll khóa học**
- Response type: `CourseDetailResponse`
- `IsEnrolled`: `false`
- `OverallProgress`: `null` (không có thông tin tiến trình)
- Các lesson: `IsUnlocked = false`, `IsCompleted = false`
- Các lesson items: `IsCompleted = false`, `CompletedAt = null`
- Schema: Giống trường hợp 1 (chưa đăng nhập)

**3. Trường hợp: User đã đăng nhập (Student role) và đã enroll khóa học**
- Response type: `CourseDetailResponse`
- `IsEnrolled`: `true`
- `OverallProgress`: `double` (0-100, phần trăm hoàn thành khóa học)
- Các lesson: 
  - `IsUnlocked`: `true/false` (dựa trên tiến trình học)
  - `IsCompleted`: `true/false` (dựa trên việc hoàn thành tất cả lesson items)
- Các lesson items:
  - `IsCompleted`: `true/false` (đã hoàn thành item chưa)
  - `CompletedAt`: `DateTime?` (thời gian hoàn thành, null nếu chưa hoàn thành)
- Schema:
```json
{
  ""message"": ""Course detail retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""name"": ""Tên khóa học"",
    ""description"": ""Mô tả khóa học"",
    ""status"": ""Open"",
    ""price"": 1000000,
    ""imageUrl"": ""/path/to/image.jpg"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-01T00:00:00Z"",
    ""isEnrolled"": true,
    ""overallProgress"": 65.5,
    ""sections"": [
      {
        ""courseId"": 34,
        ""courseSectionId"": 1,
        ""name"": ""Section 1"",
        ""description"": ""Mô tả section"",
        ""order"": 1,
        ""isActive"": true,
        ""lessons"": [
          {
            ""lessonId"": 1,
            ""courseSectionId"": 1,
            ""name"": ""Lesson 1"",
            ""description"": ""Mô tả lesson"",
            ""order"": 1,
            ""isActive"": true,
            ""isUnlocked"": true,
            ""isCompleted"": true,
            ""lessonItems"": [
              {
                ""lessonItemId"": 1,
                ""name"": ""Video 1"",
                ""order"": 1,
                ""isCompleted"": true,
                ""completedAt"": ""2024-01-15T10:30:00Z""
              },
              {
                ""lessonItemId"": 2,
                ""name"": ""Document 1"",
                ""order"": 2,
                ""isCompleted"": false,
                ""completedAt"": null
              }
            ],
            ""quiz"": {
              ""quizId"": 1,
              ""quizTitle"": ""Quiz Title"",
              ""quizDescription"": ""Quiz Description""
            }
          }
        ]
      }
    ]
  }
}
```
- **Lưu ý**: 
  - `overallProgress` được tính dựa trên số lesson đã hoàn thành / tổng số lesson
  - `isUnlocked` của lesson phụ thuộc vào tiến trình học (lesson trước đó đã hoàn thành)
  - `isCompleted` của lesson = `true` khi tất cả lesson items trong lesson đó đã hoàn thành
  - `completedAt` chỉ có giá trị khi `isCompleted = true`")]
        public async Task<ActionResult<BaseResponse<CourseDetailResponse>>> GetCourseDetail(int courseId)
        {
            try
            {
                // Tự động lấy studentId từ token nếu đã đăng nhập
                string? studentId = null;
                
                if (User.Identity?.IsAuthenticated == true)
                {
                    studentId = User.FindFirst("AccountID")?.Value;
                    
                    // Chỉ hiển thị progress nếu là Student role
                    var roles = User.FindAll(ClaimTypes.Role)
                                   .Select(c => c.Value)
                                   .ToList();
                    
                    // Nếu không phải Student role, không truyền studentId (không cần progress)
                    if (!roles.Contains("Student"))
                    {
                        studentId = null;
                    }
                }

                var result = await _courseService.GetCourseDetailAsync(courseId, studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy thông tin khóa học: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("get-lesson-item-detail")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy tất cả thông tin chi tiết của lesson item", 
            Description = @"Api dùng để lấy thông tin chi tiết của một lesson item (video, pdf, image, quiz).

**Response Schema:**
```json
{
  ""message"": ""Lấy nội dung bài học thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""lessonItemId"": 1,
    ""name"": ""Video bài học 1"",
    ""description"": ""Mô tả về video này"",
    ""content"": ""https://example.com/videos/lesson1.mp4"",
    ""itemType"": ""video""
  }
}
```

**Các loại ItemType:**
- `""video""`: Video bài học (content là URL video)
- `""pdf""`: Tài liệu PDF (content là URL file PDF)
- `""image""`: Hình ảnh (content là URL hình ảnh)
- `""quiz""`: Bài quiz (content có thể là URL hoặc JSON data)

**Lưu ý:**
- Field `content` sẽ chứa URL đầy đủ đến media file (video, pdf, image) hoặc data của quiz
- Nếu file không tồn tại, `content` sẽ là chuỗi rỗng `""""`
- Cần đăng nhập (Authorize) để sử dụng API này")]
        public async Task<ActionResult<BaseResponse<LessonItemDetail>>> GetLessonItemDetail([FromQuery]int lessonItemId)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _courseService.GetLessonItemDetailAsync(userId, lessonItemId);

            return Ok(result);
        }

        [HttpGet("get-linked-students-progress")]
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(
            Summary = "Xem tiến trình học của các học sinh đã liên kết với Parent (Parent)", 
            Description = @"Api dùng để phụ huynh xem tiến trình học tập của tất cả các học sinh đã được liên kết với tài khoản của mình.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - `SearchByStudentName` (string, optional) - Tìm kiếm theo tên học sinh
  - `StudentId` (string, optional) - Lọc theo ID học sinh
  - `CourseId` (int, optional) - Lọc theo ID khóa học
  - `MinProgress` (double, optional) - Lọc theo tiến trình tối thiểu (%)
  - `MaxProgress` (double, optional) - Lọc theo tiến trình tối đa (%)

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy tiến trình học thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""studentId"": ""student-id-123"",
        ""studentName"": ""Nguyễn Văn A"",
        ""studentEmail"": ""student@example.com"",
        ""courses"": [
          {
            ""courseId"": 34,
            ""courseName"": ""Tên khóa học"",
            ""enrolledAt"": ""2024-01-01T00:00:00Z"",
            ""overallProgress"": 65.5,
            ""sections"": []
          }
        ]
      }
    ],
    ""totalCount"": 5,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 1
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không xác định được tài khoản."",
  ""statusCode"": 401,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Parent role mới có quyền sử dụng API này
- Parent ID được lấy tự động từ JWT token
- Chỉ hiển thị các học sinh đã được liên kết với parent này
- `overallProgress` được tính dựa trên số lesson đã hoàn thành / tổng số lesson")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<StudentProgressOverviewResponse>>>> GetLinkedStudentsProgress([FromQuery] StudentProgressQueryRequest request)
        {
            try
            {
                var user = HttpContext.User;
                var parentId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(parentId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.GetLinkedStudentsProgressAsync(parentId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("course-buy-by-parent")]
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(
            Summary = "Xem danh sách khóa học Parent đã mua (Parent)", 
            Description = @"Api dùng để phụ huynh xem danh sách các khóa học mà mình đã thanh toán (mua) cho học sinh với phân trang.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `ParentEnrollmentQueryRequest`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy danh sách khóa học đã mua thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""courseId"": 34,
        ""courseName"": ""Tên khóa học"",
        ""courseDescription"": ""Mô tả khóa học"",
        ""imageUrl"": ""/path/to/image.jpg"",
        ""enrolledAt"": ""2024-01-01T00:00:00Z""
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

1. **Không xác định được tài khoản:**
```json
{
  ""message"": ""Không xác định được tài khoản."",
  ""statusCode"": 401,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Parent role mới có quyền sử dụng API này
- Parent ID được lấy tự động từ JWT token
- Kết quả chỉ trả về các khóa học mà parent đã thanh toán
- Mỗi khóa học chỉ xuất hiện một lần trong danh sách (distinct)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<ParentEnrollmentResponse>>>> GetCourseBuyByParent([FromQuery] ParentEnrollmentQueryRequest request)
        {
            var user = HttpContext.User;
            var parentId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(parentId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
            }

            request.userID = parentId;

            var result = await _courseService.GetCourseBuyByParentAsync(parentId, request);
            return Ok(result);
        }

        [HttpPut("{courseId}/approve")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = "Duyệt khóa học (Manager)", 
            Description = @"Api dùng để manager duyệt khóa học từ trạng thái `Pending` sang `Open` (cho phép công khai).

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học cần duyệt

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course approved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""name"": ""Tên khóa học"",
    ""description"": ""Mô tả khóa học"",
    ""status"": ""Open"",
    ""price"": 1000000,
    ""imageUrl"": ""/path/to/image.jpg"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-15T10:30:00Z""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Khóa học không tồn tại:**
```json
{
  ""message"": ""Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Khóa học không ở trạng thái Pending:**
```json
{
  ""message"": ""Course is not in Pending status"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Manager role mới có quyền sử dụng API này
- Chỉ có thể duyệt khóa học ở trạng thái `Pending`
- Sau khi duyệt, khóa học sẽ chuyển sang trạng thái `Open` và có thể được công khai
- `AvailableSlot` của teacher không thay đổi sau khi duyệt")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> ApproveCourse(int courseId)
        {
            try
            {
                var result = await _courseService.ApproveCourseAsync(courseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPut("{courseId}/reject")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = "Từ chối khóa học (Manager)", 
            Description = @"Api dùng để manager từ chối khóa học từ trạng thái `Pending` sang `Rejected`. Hệ thống sẽ tự động trả lại slot cho teacher.

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học cần từ chối
- Query parameter: `reason` (string, optional) - Lý do từ chối

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course rejected: Lý do từ chối"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""name"": ""Tên khóa học"",
    ""description"": ""Mô tả khóa học"",
    ""status"": ""Rejected"",
    ""price"": 1000000,
    ""imageUrl"": ""/path/to/image.jpg"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-15T10:30:00Z""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Khóa học không tồn tại:**
```json
{
  ""message"": ""Course not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Khóa học không ở trạng thái Pending:**
```json
{
  ""message"": ""Course is not in Pending status"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Logic tự động:**
- Sau khi từ chối, khóa học chuyển sang trạng thái `Rejected`
- `AvailableSlot` của teacher sẽ được tăng lên 1 (trả lại slot)
- Nếu có `reason`, message sẽ bao gồm lý do từ chối

**Lưu ý:**
- Chỉ Manager role mới có quyền sử dụng API này
- Chỉ có thể từ chối khóa học ở trạng thái `Pending`
- Slot được trả lại tự động cho teacher tạo khóa học")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> RejectCourse(int courseId, [FromQuery] string? reason = null)
        {
            try
            {
                var result = await _courseService.RejectCourseAsync(courseId, reason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}
