using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Request.CourseContent;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.CourseContent;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.Courses;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using Common.Constants;


namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly CourseRegistrationService _courseRegistrationService;
        private readonly ModuleService _moduleService;
        //private readonly ChapterService _chapterService;
        private readonly MediaService _mediaService;
        private readonly IWebHostEnvironment _environment;
        private readonly PaymentService _paymentService;

        public CourseController(
            CourseService courseService,
            CourseRegistrationService courseRegistrationService,
            ModuleService moduleService,
            //ChapterService chapterService,
            MediaService mediaService,
            IWebHostEnvironment environment,
            PaymentService paymentService)
        {
            _mediaService = mediaService;
            _environment = environment;
            _moduleService = moduleService;
            //_chapterService = chapterService;
            _courseService = courseService;
            _courseRegistrationService = courseRegistrationService;
            _paymentService = paymentService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo khóa học (Teacher)")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> CreateCourse([FromForm] CourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.ImageFile != null && FileUploadHelper.IsValidImageFile(request.ImageFile))
            {
                request.ImageUrl = await FileUploadHelper.UploadCourseImageAsync(request.ImageFile, _environment.WebRootPath);
            }

            var userId = User.FindFirst("AccountID")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<object>(
                    $"User ID not found in token", 
                    StatusCodeEnum.Unauthorized_401, 
                    null));
            }

            var result = await _courseService.CreateCourseAsync(request, userId);

            return Ok(result);
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Lấy danh sách các khóa học")]
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
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("{courseId}/approve")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(Summary = "Duyệt khóa học (Manager)")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> ApproveCourse(long courseId)
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
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("{courseId}/reject")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(Summary = "Từ chối khóa học (Manager)")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> RejectCourse(long courseId, [FromBody] string? reason)
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
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("my-registrations")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(Summary = "Lấy danh sách khóa học đã đăng ký của chính mình (Student)")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseRegistrationResponse>>>> GetMyRegistrations()
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseRegistrationService.GetStudentRegistrationsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy danh sách khóa học đã đăng ký: {ex.Message}",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("progress")]
        [SwaggerOperation(Summary = "Lấy thông tin tiến trình khóa học đã đăng ký của sinh viên theo studentId và courseId (Parent/Student)")]
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
        [Authorize(Roles = "Student")]
        [SwaggerOperation(Summary = "Đánh dấu hoàn thành lesson item (Student)")]
        public async Task<ActionResult<BaseResponse<bool>>> CompleteLessonItem([FromQuery] int lessonItemId)
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseRegistrationService.CompleteLessonItemAsync(userId, lessonItemId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi hoàn thành lesson item: {ex.Message}",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("section/create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo các course section (Teacher)")]
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

            var result = await _courseService.CreateCourseSectionAsync(request);
            return Created("course section", result);
        }

        [HttpPost("lesson/create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo các course lesson (Teacher)")]
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

            var result = await _courseService.CreateLessonAsync(request);
            return Created("lesson", result);
        }

        [HttpPost("lesson-item/create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo các course lessonitem (Teacher)")]
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

                        fileUrl = await FileUploadHelper.UploadLessonDocumentAsync(request.File, _environment.WebRootPath);
                    }
                    else if (request.ItemType.Equals("video", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!FileUploadHelper.IsValidLessonVideo(request.File))
                            throw new ArgumentException("Invalid video file");

                        fileUrl = await FileUploadHelper.UploadLessonVideoAsync(request.File, _environment.WebRootPath);
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
        
        [HttpGet("get-all-similar-courses")]
        [SwaggerOperation(Summary = "Lấy danh sách các khóa học tương tự")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetAllSimilarCourses([FromQuery] SimilarCourseRequest request)
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

            var result = await _courseService.GetAllSimilarCoursesAsync(request.CourseId, request.Score);
            return Ok(result);
        }

        [HttpGet("my-create-course")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Lấy tất cả khóa học do teacher đã tạo (Teacher)")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetMyCreatedCourses()
        {
            var user = HttpContext.User;
            var teacherId = user.FindFirst("AccountID")?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", Common.Constants.StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _courseService.GetTeacherCoursesAsync(teacherId);
            return Ok(result);
        }

        [HttpGet("{courseId}")]
        [SwaggerOperation(Summary = "Lấy tất cả thông tin chi tiết của khóa học (bao gồm sections, lessons, lesson items và tiến trình học của Student)")]
        public async Task<ActionResult<BaseResponse<CourseDetailResponse>>> GetCourseDetail(int courseId, [FromQuery] string? studentId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    var user = HttpContext.User;
                    studentId = user.FindFirst("AccountID")?.Value;
                }

                var result = await _courseService.GetCourseDetailAsync(courseId, studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy thông tin khóa học: {ex.Message}",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}
