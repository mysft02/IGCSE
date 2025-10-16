using DTOs.Request.Courses;
using DTOs.Response.Courses;
using DTOs.Request.CourseRegistration;
using DTOs.Response.CourseRegistration;
using DTOs.Response.CourseContent;
using DTOs.Request.CourseContent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service;
using DTOs.Response.Accounts;
using Common.Utils;

namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly CourseRegistrationService _courseRegistrationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CourseController(CourseService courseService, CourseRegistrationService courseRegistrationService, IWebHostEnvironment webHostEnvironment)
        {
            _courseService = courseService;
            _courseRegistrationService = courseRegistrationService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Existing course management endpoints
        [HttpPost("create")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> CreateCourse([FromForm] CourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
        }

            try
        {
                if (request.ImageFile != null && FileUploadHelper.IsValidImageFile(request.ImageFile))
                {
                    var imageUrl = await FileUploadHelper.UploadCourseImageAsync(request.ImageFile, _webHostEnvironment.WebRootPath);
                    request.ImageUrl = imageUrl;
                }

                var result = await _courseService.CreateCourseAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<BaseResponse<PagedResponse<CourseResponse>>>> GetAllCourses([FromQuery] CourseListQuery query)
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
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("registrations/{studentId}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseRegistrationResponse>>>> GetStudentRegistrations(string studentId)
        {
            try
            {
                var result = await _courseRegistrationService.GetStudentRegistrationsAsync(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("content/{courseKeyId}/section/{courseSectionId}")]
        public async Task<ActionResult<BaseResponse<CourseSectionResponse>>> GetCourseContent(long courseKeyId, long courseSectionId)
        {
            try
            {
                var result = await _courseRegistrationService.GetCourseContentAsync(courseKeyId, courseSectionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.NotFound_404,
                    null
                ));
            }
        }

        [HttpGet("progress")]
        public async Task<ActionResult<BaseResponse<StudentProgressResponse>>> GetStudentProgressByStudentAndCourse([FromQuery] string studentId, [FromQuery] long courseId)
        {
            try
            {
                var result = await _courseRegistrationService.GetStudentProgressAsync(studentId, courseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.NotFound_404,
                    null
                ));
            }
        }

        [HttpPost("complete-lesson-item")]
        public async Task<ActionResult<BaseResponse<bool>>> CompleteLessonItem([FromQuery] int courseKeyId, [FromQuery] int lessonItemId)
        {
            try
            {
                var result = await _courseRegistrationService.CompleteLessonItemAsync(courseKeyId, lessonItemId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        // Course Content Management endpoints
        [HttpPost("section/create")]
        public async Task<ActionResult<BaseResponse<CourseSectionResponse>>> CreateCourseSection([FromBody] CourseSectionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                var result = await _courseService.CreateCourseSectionAsync(request);
                return Created("course section", result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
            }

        [HttpPost("lesson/create")]
        public async Task<ActionResult<BaseResponse<LessonResponse>>> CreateLesson([FromBody] LessonRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
        }

            try
        {
                var result = await _courseService.CreateLessonAsync(request);
                return Created("lesson", result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("lesson-item/create")]
        public async Task<ActionResult<BaseResponse<LessonItemResponse>>> CreateLessonItem([FromForm] LessonItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                // If a file is provided, upload based on ItemType and set Content to URL
                if (request.File != null && request.File.Length > 0)
                {
                    string? fileUrl = null;

                    if (request.ItemType.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!FileUploadHelper.IsValidLessonDocument(request.File))
                            throw new ArgumentException("Invalid PDF file");

                        fileUrl = await FileUploadHelper.UploadLessonDocumentAsync(request.File, _webHostEnvironment.WebRootPath);
                    }
                    else if (request.ItemType.Equals("video", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!FileUploadHelper.IsValidLessonVideo(request.File))
                            throw new ArgumentException("Invalid video file");

                        fileUrl = await FileUploadHelper.UploadLessonVideoAsync(request.File, _webHostEnvironment.WebRootPath);
                    }

                    if (!string.IsNullOrEmpty(fileUrl))
                    {
                        request.Content = fileUrl;
                    }
                }

                var result = await _courseService.CreateLessonItemAsync(request);
                return Created("lesson item", result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
    }
}

        [HttpGet("{courseId}/sections")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseSectionResponse>>>> GetCourseSections(long courseId)
        {
            try
            {
                var result = await _courseService.GetCourseSectionsAsync(courseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("lesson/{lessonId}/items")]
        public async Task<ActionResult<BaseResponse<IEnumerable<LessonItemResponse>>>> GetLessonItems(long lessonId)
        {
            try
            {
                var result = await _courseService.GetLessonItemsAsync(lessonId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("redeem-key")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<BaseResponse<string>>> RedeemCourseKey([FromBody] string courseKeyValue)
        {
            try
            {
                // Lấy thông tin student từ JWT token
                var user = HttpContext.User;
                var studentId = user.FindFirst("AccountID")?.Value;
                var roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList();

                if (string.IsNullOrEmpty(studentId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản học sinh.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
                }

                if (!roles.Contains("Student"))
                {
                    return Forbid("Chỉ tài khoản học sinh mới được sử dụng mã khóa học!");
                }

                // Tìm mã khóa theo KeyValue và kiểm tra tính hợp lệ
                var courseKeys = await _courseRegistrationService.GetAvailableCourseKeysAsync();
                var keyObj = courseKeys.FirstOrDefault(x => x.KeyValue == courseKeyValue);

                if (keyObj == null)
                {
                    return NotFound(new BaseResponse<string>("Mã khóa không tồn tại, đã được sử dụng, hoặc không hợp lệ.", Common.Constants.StatusCodeEnum.NotFound_404, null));
                }

                // Kiểm tra xem student đã có khóa học này chưa
                var allCourseKeys = await _courseRegistrationService.GetAllCourseKeysAsync();
                var existingKey = allCourseKeys.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == keyObj.CourseId);
                if (existingKey != null)
                {
                    return BadRequest(new BaseResponse<string>("Bạn đã có khóa học này rồi!", Common.Constants.StatusCodeEnum.BadRequest_400, null));
                }

                // Gán khóa học cho student và cập nhật trạng thái
                keyObj.StudentId = studentId;
                keyObj.Status = "redeemed";
                keyObj.UpdatedAt = DateTime.UtcNow;
                await _courseRegistrationService.UpdateCourseKeyAsync(keyObj);

                // Khởi tạo tiến trình học tập cho student
                await _courseRegistrationService.InitializeCourseProgressAsync(keyObj.CourseKeyId);

                return Ok(new BaseResponse<string>("Kích hoạt khóa học thành công!", Common.Constants.StatusCodeEnum.OK_200, keyObj.KeyValue));
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi kích hoạt khóa học: {ex.Message}",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}
