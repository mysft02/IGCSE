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

        public CourseController(
            CourseService courseService,
            ModuleService moduleService,
            MediaService mediaService,
            IWebHostEnvironment environment,
            PaymentService paymentService)
        {
            _mediaService = mediaService;
            _environment = environment;
            _moduleService = moduleService;
            _courseService = courseService;
            _paymentService = paymentService;
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Lấy danh sách các khóa học", Description = "Lấy danh sách khóa học với các trạng thái: " +
            "`1` là `open`(đã duyệt); " +
            "`2` là `pending`(chưa được duyệt để public) ")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseResponse>>>> GetAllCourses([FromQuery] CourseListQuery query)
        {
            try
            {
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
        [SwaggerOperation(Summary = "Lấy danh sách khóa học đã đăng ký của chính mình (Student)", Description = "Api dùng để lấy danh sách các khóa học đã enroll của học sinh ")]
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
        [SwaggerOperation(Summary = "Lấy tất cả khóa học do teacher đã tạo (Teacher)", Description = "Api dùng để lấy danh sách các khóa học giáo viên đã tạo")]
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
        [SwaggerOperation(Summary = "Lấy danh sách các khóa học tương tự", Description = "Api dùng để lấy danh sách các khóa học tương tự với khóa học hiện tại ")]
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
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<object>>> GetCourseDetail(int courseId)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var result = new object();
            if(userRole != null && userRole == "Student")
            {
                result = await _courseService.GetCourseDetailForStudentAsync(courseId, userId);
            }
            else
            {
                result = await _courseService.GetCourseDetailAsync(courseId);
            }

            return Ok(result);
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
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _courseService.GetLessonItemDetailAsync(userId, lessonItemId, userRole);

            return Ok(result);
        }

        [HttpGet("get-linked-students-progress")]
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(Summary = "Xem tiến trình học của các học sinh đã liên kết với Parent (Parent)", Description = "Api dùng để lấy xem được tiến độ học các khóa học đã enroll của học sinh ")]
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
        [SwaggerOperation(Summary = "Xem danh sách khóa học Parent đã mua (Parent)", Description = "Api dùng để lấy danh sách các khóa học mà phụ huynh đã thanh toán ")]
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
        [SwaggerOperation(Summary = "Duyệt khóa học (Manager)", Description = "Api dùng để duyệt khóa học từ trạng thái Pending sang Open. Không thay đổi AvailableSlot")]
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
        [SwaggerOperation(Summary = "Từ chối khóa học (Manager)", Description = "Api dùng để từ chối khóa học từ trạng thái Pending sang Rejected. AvailableSlot sẽ +1 để trả lại slot cho teacher.")]
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
