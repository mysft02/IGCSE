using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Request.CourseContent;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.DTOs.Response.CourseContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using Common.Constants;
using System.Security.Claims;
using Common.Utils;


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
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Lấy danh sách các khóa học",
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
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _courseService.GetCoursesPagedAsync(query, userRole);
            return Ok(result);
        }

        [HttpGet("get-course-certificates")]
        [Authorize(Roles = "Parent, Student")]
        [SwaggerOperation(
            Summary = "Lấy toàn bộ chứng chỉ hoàn thành khoá học",
            Description = @"Api dùng để lấy danh sách chứng chỉ khoá học của học sinh dùng cho ""Parent"" và ""Student""

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 1) - Số trang
  - `PageSize` (int, mặc định: 10) - Số lượng item mỗi trang
  - `StudentId` (int, optional) - Lọc theo ID khóa học

**Lưu ý:**
- Nếu Role là ""Parent"" thì cần truyền ""StudentId"" để lấy thông tin của học sinh
- Nếu Role là ""Student"" thì k cần truyền gì hết api sẽ tự lấy ""StudentId"" từ token

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""string"",
  ""statusCode"": 100,
  ""data"": {
    ""items"": [
      {
        ""courseId"": 0,
        ""userId"": ""string"",
        ""imageUrl"": ""string"",
        ""createdAt"": ""2025-12-04T14:54:55.699Z""
      }
    ],
    ""totalCount"": 0,
    ""page"": 0,
    ""size"": 0,
    ""totalPages"": 0,
    ""hasNextPage"": true,
    ""hasPreviousPage"": true,
    ""currentPage"": 0
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
- Kết quả được sắp xếp mặc định theo thời gian tạo mới nhất")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseCertificateResponse>>>> GetAllCertificates([FromQuery] CourseCertificatesQuery request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _courseService.GetCourseCertificates(userRole, userId, request);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Tạo mới khóa học (Teacher)", 
            Description = @"Api dùng để giáo viên tạo khóa học. Hệ thống sẽ tự động kiểm tra available slot và trừ slot khi tạo thành công.

**Request:**
- Body (multipart/form-data hoặc JSON):
```json
{
  ""name"": ""Tên khóa học"",
  ""description"": ""Mô tả khóa học"",
  ""price"": 1000000,
  ""moduleId"": 1,
  ""status"": ""Pending"",
  ""imageUrl"": ""https://example.com/image.jpg"",
  ""imageFile"": null
}
```
  - `name` (string, required, max 200) - Tên khóa học
  - `description` (string, optional, max 1000) - Mô tả khóa học
  - `price` (decimal, required) - Giá khóa học
  - `moduleId` (int, required) - ID của module
  - `status` (string, required) - Trạng thái khóa học (mặc định: ""Pending"")
  - `imageUrl` (string, optional) - Link hình ảnh khóa học (nếu upload file thì để null)
  - `imageFile` (IFormFile, optional) - File hình ảnh khóa học (nếu dùng link thì để null)

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course created successfully"",
  ""statusCode"": 201,
  ""data"": {
    ""courseId"": 1,
    ""name"": ""Tên khóa học"",
    ""description"": ""Mô tả khóa học"",
    ""status"": ""Pending"",
    ""price"": 1000000,
    ""imageUrl"": ""https://example.com/api/media/get-media?imagePath=courses/images/xxx.jpg"",
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-01T00:00:00Z"",
    ""createdBy"": ""user-id-123""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Chưa mua gói:**
```json
{
  ""message"": ""Bạn chưa mua gói để tạo mới khoá học. Vui lòng mua gói trước."",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Hết slot:**
```json
{
  ""message"": ""Bạn không còn suất tạo mới khoá học. Vui lòng mua thêm gói."",
  ""statusCode"": 400,
  ""data"": null
}
```

3. **Module không tồn tại:**
```json
{
  ""message"": ""Module not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Logic tự động:**
- Kiểm tra available slot của teacher
- Trừ 1 slot khi tạo thành công
- Tự động tạo FinalQuiz cho khóa học
- Tạo embedding data cho khóa học
- Khóa học được tạo với status ""Pending"" để manager duyệt

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Có thể upload image bằng file hoặc link URL
- Nếu không có image, sẽ dùng image mặc định")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> CreateCourse([FromForm] CourseRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.CreateCourseAsync(request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpPut("{courseId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Cập nhật khóa học (Teacher)",
            Description = @"Api cho phép giáo viên cập nhật thông tin khóa học do mình tạo.

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học cần cập nhật
- Body (multipart/form-data):
```json
{
  ""name"": ""Tên khóa học đã cập nhật"",
  ""description"": ""Mô tả khóa học đã cập nhật"",
  ""price"": 1500000,
  ""moduleId"": 1,
  ""imageUrl"": ""https://example.com/image.jpg"",
  ""imageFile"": null
}
```

**Các trường có thể cập nhật:**
- `name` (string, required) - Tên khóa học
- `description` (string, optional) - Mô tả khóa học
- `price` (decimal, required) - Giá khóa học
- `moduleId` (int, required) - ID module
- `imageUrl` (string, optional) - Link hình ảnh (nếu upload file thì để null)
- `imageFile` (IFormFile, optional) - File hình ảnh (nếu dùng link thì để null)

**Điều kiện:**
- Chỉ có thể cập nhật khóa học do chính mình tạo
- **Không thể cập nhật** khóa học đã được duyệt (status = `Open`)
- Chỉ có thể cập nhật khi khóa học ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`

**Logic tự động:**
- Tự động cập nhật `UpdatedAt` và `UpdatedBy`
- Tự động tạo lại embedding data cho khóa học (nếu có thay đổi)

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course updated successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 1,
    ""name"": ""Tên khóa học đã cập nhật"",
    ""description"": ""Mô tả khóa học đã cập nhật"",
    ""status"": ""Pending"",
    ""price"": 1500000,
    ""imageUrl"": ""/uploads/courses/image.jpg"",
    ""moduleId"": 1,
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-02T00:00:00Z""
  }
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Khóa học đã được duyệt, không thể chỉnh sửa."",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Có thể upload image bằng file hoặc link URL (chỉ chọn 1 trong 2)
- Nếu không cung cấp image mới, sẽ giữ nguyên image cũ"
        )]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> UpdateCourse(int courseId, [FromForm] CourseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<object>(
                        "Dữ liệu không hợp lệ",
                        StatusCodeEnum.BadRequest_400,
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    ));
                }

                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.UpdateCourseAsync(courseId, request, userId);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi cập nhật khóa học: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("{courseId}/submit")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Gửi khóa học sang Manager duyệt (Teacher)",
            Description = "Chỉ gửi được khi khóa học ở trạng thái Draft hoặc Rejected. Khi gửi duyệt sẽ trừ slot của giáo viên."
        )]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> SubmitCourseForApproval(int courseId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.SubmitCourseForApprovalAsync(courseId, userId);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi gửi duyệt khóa học: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpDelete("{courseId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Xóa khóa học (Teacher)",
            Description = @"Api cho phép giáo viên xóa khóa học do mình tạo.

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học cần xóa

**Điều kiện:**
- Chỉ có thể xóa khóa học do chính mình tạo
- **Không thể xóa** khóa học đã có học viên đăng ký (đã có enrollment)
- Có thể xóa khóa học ở bất kỳ trạng thái nào (Draft, Pending, Rejected, Open)

**Logic tự động:**
- Tự động hoàn trả 1 slot cho giáo viên (tăng `AvailableSlot` lên 1)
- Xóa tất cả dữ liệu liên quan đến khóa học (sections, lessons, lesson items, etc.)

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course deleted successfully"",
  ""statusCode"": 200,
  ""data"": ""Khóa học đã được xóa thành công.""
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Không thể xóa khóa học khi đã có học viên đăng ký."",
  ""statusCode"": 400,
  ""data"": null
}
```

**Các trường hợp lỗi có thể xảy ra:**
- `Course not found` - Khóa học không tồn tại
- `Bạn không có quyền xóa khóa học này.` - Không phải người tạo khóa học
- `Không thể xóa khóa học khi đã có học viên đăng ký.` - Đã có học viên đăng ký

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Hành động xóa là **vĩnh viễn** và không thể hoàn tác
- Slot sẽ được hoàn trả ngay sau khi xóa thành công"
        )]
        public async Task<ActionResult<BaseResponse<string>>> DeleteCourse(int courseId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.DeleteCourseAsync(courseId, userId);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi xóa khóa học: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("{courseId}/sections")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Tạo mới section cho khóa học (Teacher)",
            Description = @"Api dùng để giáo viên tạo section mới cho khóa học của mình.

**Request:**
- Path parameter: `courseId` (int) - ID của khóa học
- Body:
```json
{
  ""name"": ""Section 1"",
  ""description"": ""Mô tả section"",
  ""courseId"": 1,
  ""order"": 1,
  ""isActive"": 1
}
```

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course section created successfully"",
  ""statusCode"": 201,
  ""data"": {
    ""courseId"": 1,
    ""courseSectionId"": 1,
    ""name"": ""Section 1"",
    ""description"": ""Mô tả section"",
    ""order"": 1,
    ""isActive"": true
  }
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Chỉ có thể tạo section cho khóa học do chính mình tạo")]
        public async Task<ActionResult<BaseResponse<CourseSectionResponse>>> CreateCourseSection(int courseId, [FromBody] CourseSectionRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.CreateCourseSectionAsync(courseId, request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpPut("sections/{sectionId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Cập nhật section (Teacher)",
            Description = @"Api cho phép giáo viên cập nhật thông tin section của khóa học.

**Request:**
- Path parameter: `sectionId` (int) - ID của section cần cập nhật
- Body:
```json
{
  ""name"": ""Section 1 đã cập nhật"",
  ""description"": ""Mô tả section đã cập nhật"",
  ""order"": 1,
  ""isActive"": 1
}
```

**Điều kiện:**
- Chỉ có thể cập nhật section thuộc khóa học do chính mình tạo
- **Không thể cập nhật** khi khóa học đã được duyệt (status = `Open`)
- Chỉ có thể cập nhật khi khóa học ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course section updated successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 1,
    ""courseSectionId"": 1,
    ""name"": ""Section 1 đã cập nhật"",
    ""description"": ""Mô tả section đã cập nhật"",
    ""order"": 1,
    ""isActive"": true
  }
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token"
        )]
        public async Task<ActionResult<BaseResponse<CourseSectionResponse>>> UpdateCourseSection(int sectionId, [FromBody] CourseSectionRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.UpdateCourseSectionAsync(sectionId, request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpDelete("sections/{sectionId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Xóa section (Teacher)",
            Description = @"Api cho phép giáo viên xóa section của khóa học.

**Request:**
- Path parameter: `sectionId` (int) - ID của section cần xóa

**Điều kiện:**
- Chỉ có thể xóa section thuộc khóa học do chính mình tạo
- **Không thể xóa** khi khóa học đã được duyệt (status = `Open`)
- Xóa section sẽ xóa tất cả lessons và lesson items bên trong

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Course section deleted successfully"",
  ""statusCode"": 200,
  ""data"": ""Section đã được xóa thành công.""
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Hành động xóa là **vĩnh viễn** và không thể hoàn tác"
        )]
        public async Task<ActionResult<BaseResponse<string>>> DeleteCourseSection(int sectionId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.DeleteCourseSectionAsync(sectionId, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpPost("sections/{sectionId}/lessons")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Tạo mới lesson cho section (Teacher)",
            Description = @"Api dùng để giáo viên tạo lesson mới cho section của khóa học.

**Request:**
- Path parameter: `sectionId` (int) - ID của section
- Body:
```json
{
  ""name"": ""Lesson 1"",
  ""description"": ""Mô tả lesson"",
  ""courseSectionId"": 1,
  ""order"": 1,
  ""isActive"": 1
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Chỉ có thể tạo lesson cho section thuộc khóa học do chính mình tạo")]
        public async Task<ActionResult<BaseResponse<LessonResponse>>> CreateLesson(int sectionId, [FromBody] LessonRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.CreateLessonAsync(sectionId, request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpPut("lessons/{lessonId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Cập nhật lesson (Teacher)",
            Description = @"Api cho phép giáo viên cập nhật thông tin lesson của section.

**Request:**
- Path parameter: `lessonId` (int) - ID của lesson cần cập nhật
- Body:
```json
{
  ""name"": ""Lesson 1 đã cập nhật"",
  ""description"": ""Mô tả lesson đã cập nhật"",
  ""order"": 1,
  ""isActive"": 1
}
```

**Điều kiện:**
- Chỉ có thể cập nhật lesson thuộc khóa học do chính mình tạo
- **Không thể cập nhật** khi khóa học đã được duyệt (status = `Open`)
- Chỉ có thể cập nhật khi khóa học ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lesson updated successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""lessonId"": 1,
    ""courseSectionId"": 1,
    ""name"": ""Lesson 1 đã cập nhật"",
    ""description"": ""Mô tả lesson đã cập nhật"",
    ""order"": 1,
    ""isActive"": true
  }
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token"
        )]
        public async Task<ActionResult<BaseResponse<LessonResponse>>> UpdateLesson(int lessonId, [FromBody] LessonRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.UpdateLessonAsync(lessonId, request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpDelete("lessons/{lessonId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Xóa lesson (Teacher)",
            Description = @"Api cho phép giáo viên xóa lesson của section.

**Request:**
- Path parameter: `lessonId` (int) - ID của lesson cần xóa

**Điều kiện:**
- Chỉ có thể xóa lesson thuộc khóa học do chính mình tạo
- **Không thể xóa** khi khóa học đã được duyệt (status = `Open`)
- Xóa lesson sẽ xóa tất cả lesson items bên trong

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lesson deleted successfully"",
  ""statusCode"": 200,
  ""data"": ""Lesson đã được xóa thành công.""
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Hành động xóa là **vĩnh viễn** và không thể hoàn tác"
        )]
        public async Task<ActionResult<BaseResponse<string>>> DeleteLesson(int lessonId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.DeleteLessonAsync(lessonId, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpPost("lessons/{lessonId}/items")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Tạo mới lesson item cho lesson (Teacher)",
            Description = @"Api dùng để giáo viên tạo lesson item mới (video, pdf, image, text) cho lesson.

**Request:**
- Path parameter: `lessonId` (int) - ID của lesson
- Body (multipart/form-data):
  - `name` (string, required) - Tên lesson item
  - `description` (string, optional) - Mô tả
  - `content` (string, optional) - Nội dung/URL (nếu upload file thì để null)
  - `itemType` (string, optional) - Loại: video, pdf, image, text (tự động detect từ file)
  - `lessonId` (int, required) - ID của lesson
  - `order` (int, required) - Thứ tự
  - `file` (IFormFile, optional) - File để upload (video/pdf/image)

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- Hỗ trợ upload: video (mp4, webm, ogg), PDF, image (jpg, png, gif, webp)
- Nếu upload file, hệ thống sẽ tự động detect loại file và set itemType")]
        public async Task<ActionResult<BaseResponse<LessonItemResponse>>> CreateLessonItem(int lessonId, [FromForm] LessonItemRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.CreateLessonItemAsync(lessonId, request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpPut("lesson-items/{lessonItemId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Cập nhật lesson item (Teacher)",
            Description = @"Api cho phép giáo viên cập nhật thông tin lesson item (video, pdf, image, text).

**Request:**
- Path parameter: `lessonItemId` (int) - ID của lesson item cần cập nhật
- Body (multipart/form-data):
  - `name` (string, required) - Tên lesson item
  - `description` (string, optional) - Mô tả
  - `content` (string, optional) - Nội dung/URL (nếu upload file mới thì để null)
  - `itemType` (string, optional) - Loại: video, pdf, image, text
  - `order` (int, required) - Thứ tự
  - `file` (IFormFile, optional) - File mới để upload (video/pdf/image). Nếu upload file mới, file cũ sẽ bị xóa

**Điều kiện:**
- Chỉ có thể cập nhật lesson item thuộc khóa học do chính mình tạo
- **Không thể cập nhật** khi khóa học đã được duyệt (status = `Open`)
- Chỉ có thể cập nhật khi khóa học ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`

**Logic tự động:**
- Nếu upload file mới, file cũ sẽ tự động bị xóa
- Tự động detect loại file và set itemType

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lesson item updated successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""lessonItemId"": 1,
    ""lessonId"": 1,
    ""name"": ""Lesson Item đã cập nhật"",
    ""description"": ""Mô tả đã cập nhật"",
    ""content"": ""/uploads/lessons/video.mp4"",
    ""itemType"": ""video"",
    ""order"": 1
  }
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Hỗ trợ upload: video (mp4, webm, ogg), PDF, image (jpg, png, gif, webp)"
        )]
        public async Task<ActionResult<BaseResponse<LessonItemResponse>>> UpdateLessonItem(int lessonItemId, [FromForm] LessonItemRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.UpdateLessonItemAsync(lessonItemId, request, userId);
                return StatusCode((int)result.StatusCode, result);
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

        [HttpDelete("lesson-items/{lessonItemId}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Xóa lesson item (Teacher)",
            Description = @"Api cho phép giáo viên xóa lesson item.

**Request:**
- Path parameter: `lessonItemId` (int) - ID của lesson item cần xóa

**Điều kiện:**
- Chỉ có thể xóa lesson item thuộc khóa học do chính mình tạo
- **Không thể xóa** khi khóa học đã được duyệt (status = `Open`)

**Logic tự động:**
- Tự động xóa file đính kèm (nếu có) khi xóa lesson item

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lesson item deleted successfully"",
  ""statusCode"": 200,
  ""data"": ""Lesson item đã được xóa thành công.""
}
```

**Lưu ý:**
- Chỉ Teacher role mới có quyền sử dụng API này
- User ID được lấy tự động từ JWT token
- Hành động xóa là **vĩnh viễn** và không thể hoàn tác
- File đính kèm sẽ bị xóa khỏi server"
        )]
        public async Task<ActionResult<BaseResponse<string>>> DeleteLessonItem(int lessonItemId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.DeleteLessonItemAsync(lessonItemId, userId);
                return StatusCode((int)result.StatusCode, result);
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
        ""updatedAt"": ""2024-01-15T10:30:00Z"",
        ""likeCount"": 11,
        ""unlikeCount"": 2,
        ""isLikedByCurrentUser"": true,
        ""isUnlikedByCurrentUser"": false
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
- Nếu user đã đăng nhập, `isLikedByCurrentUser` và `isUnlikedByCurrentUser` sẽ hiển thị trạng thái reaction của user
- Nếu user chưa đăng nhập, `isLikedByCurrentUser` và `isUnlikedByCurrentUser` sẽ luôn là `false`
- Mặc định sắp xếp theo ngày tạo mới nhất (desc)
- Có thể kết hợp nhiều filter cùng lúc")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseFeedbackResponse>>>> GetCourseFeedbacks(int courseId, [FromQuery] CourseFeedbackQueryRequest request)
        {
            try
            {
                string? currentUserId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    currentUserId = User.FindFirst("AccountID")?.Value;
                }

                var result = await _courseFeedbackService.GetCourseFeedbacksPagedAsync(courseId, request, currentUserId);
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

        [HttpPost("{courseId}/feedbacks/{feedbackId}/like")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Like feedback", 
            Description = @"Api dùng để user like một feedback. Logic hoạt động giống Shopee:
- Nếu chưa like/unlike: tạo like mới
- Nếu đã like: bỏ like (toggle off)
- Nếu đã unlike: đổi từ unlike sang like

**Request:**
- Path parameters:
  - `courseId` (int) - ID của khóa học
  - `feedbackId` (int) - ID của feedback cần like

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Thêm Like thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""courseFeedbackId"": 1,
    ""courseId"": 34,
    ""studentId"": ""user-id-123"",
    ""studentName"": ""Nguyễn Văn A"",
    ""rating"": 5,
    ""comment"": ""Khóa học rất hay"",
    ""createdAt"": ""2024-01-15T10:30:00Z"",
    ""updatedAt"": ""2024-01-15T10:30:00Z"",
    ""likeCount"": 11,
    ""unlikeCount"": 2,
    ""isLikedByCurrentUser"": true,
    ""isUnlikedByCurrentUser"": false
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Feedback không tồn tại:**
```json
{
  ""message"": ""Lỗi khi like feedback: Feedback not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Mỗi user chỉ có thể like hoặc unlike một lần (không thể cùng lúc cả hai)
- Có thể toggle (bật/tắt) like bằng cách click lại")]
        public async Task<ActionResult<BaseResponse<CourseFeedbackResponse>>> LikeFeedback(int courseId, int feedbackId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseFeedbackService.ReactToFeedbackAsync(feedbackId, userId, "Like");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi like feedback: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("{courseId}/feedbacks/{feedbackId}/unlike")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Unlike feedback", 
            Description = @"Api dùng để user unlike một feedback. Logic hoạt động giống Shopee:
- Nếu chưa like/unlike: tạo unlike mới
- Nếu đã unlike: bỏ unlike (toggle off)
- Nếu đã like: đổi từ like sang unlike

**Request:**
- Path parameters:
  - `courseId` (int) - ID của khóa học
  - `feedbackId` (int) - ID của feedback cần unlike

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Thêm Unlike thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""courseFeedbackId"": 1,
    ""courseId"": 34,
    ""studentId"": ""user-id-123"",
    ""studentName"": ""Nguyễn Văn A"",
    ""rating"": 5,
    ""comment"": ""Khóa học rất hay"",
    ""createdAt"": ""2024-01-15T10:30:00Z"",
    ""updatedAt"": ""2024-01-15T10:30:00Z"",
    ""likeCount"": 10,
    ""unlikeCount"": 3,
    ""isLikedByCurrentUser"": false,
    ""isUnlikedByCurrentUser"": true
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Feedback không tồn tại:**
```json
{
  ""message"": ""Lỗi khi unlike feedback: Feedback not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Mỗi user chỉ có thể like hoặc unlike một lần (không thể cùng lúc cả hai)
- Có thể toggle (bật/tắt) unlike bằng cách click lại")]
        public async Task<ActionResult<BaseResponse<CourseFeedbackResponse>>> UnlikeFeedback(int courseId, int feedbackId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseFeedbackService.ReactToFeedbackAsync(feedbackId, userId, "Unlike");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi unlike feedback: {ex.Message}",
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
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _courseService.GetStudentRegistrationsAsync(userId, request, userRole);
            return Ok(result);
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

        [HttpGet("get-introduced-courses")]
        [Authorize(Roles = "Parent, Student")]
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
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetAllSimilarCoursesWithFinalQuizResultList()
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _courseService.GetSimilarCourses(userId, userRole);
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
                result = await _courseService.GetCourseDetailAsync(courseId, userId, userRole);
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
