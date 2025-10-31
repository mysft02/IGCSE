using BusinessObject.DTOs.Request.Chapters;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Response.Chapters;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.DTOs.Request.CourseContent;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.CourseContent;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;


namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly CourseRegistrationService _courseRegistrationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PaymentService _paymentService;
        private readonly ModuleService _moduleService;
        private readonly ChapterService _chapterService;

        public CourseController(
            CourseService courseService,
            CourseRegistrationService courseRegistrationService,
            IWebHostEnvironment webHostEnvironment,
            PaymentService paymentService,
            ModuleService moduleService,
            ChapterService chapterService)
        {
            _courseService = courseService;
            _courseRegistrationService = courseRegistrationService;
            _webHostEnvironment = webHostEnvironment;
            _paymentService = paymentService;
            _moduleService = moduleService;
            _chapterService = chapterService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo khóa học (Teacher)")]
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

            if (request.ImageFile != null && FileUploadHelper.IsValidImageFile(request.ImageFile))
            {
                request.ImageUrl = await FileUploadHelper.UploadCourseImageAsync(request.ImageFile, _webHostEnvironment.WebRootPath);
            }

            var result = await _courseService.CreateCourseAsync(request);
            return Ok(result);
        }

        [HttpGet("all")]
        //[Authorize(Roles = "Manager")]
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

        [HttpGet("pending")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(Summary = "Lấy danh sách khóa học đang pending để duyệt/từ chối (Manager)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseResponse>>>> GetPendingCourses([FromQuery] CourseListQuery query)
        {
            try
            {
                var result = await _courseService.GetPendingCoursesPagedAsync(query.Page, query.PageSize, query.SearchByName);
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
                // Lấy thông tin user từ JWT token
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
        [HttpGet("registrations/{studentId}")]
        [Authorize(Roles = "Parent")]
        [SwaggerOperation(Summary = "Lấy danh sách khóa học đã đăng ký của sinh viên theo studentId (Parent)")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseRegistrationResponse>>>> GetStudentRegistrations(string studentId)
        {
            var result = await _courseRegistrationService.GetStudentRegistrationsAsync(studentId);
            return Ok(result);
        }

        [HttpGet("content/{courseKeyId}/section/{courseSectionId}")]
        public async Task<ActionResult<BaseResponse<CourseSectionResponse>>> GetCourseContent(long courseKeyId, long courseSectionId)
        {
            var result = await _courseRegistrationService.GetCourseContentAsync(courseKeyId, courseSectionId);
            return Ok(result);
        }

        [HttpGet("progress")]
        [SwaggerOperation(Summary = "Lấy thông tin tiến trình khóa học đã đăng ký của sinh viên theo studentId và courseId (Parent)")]
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
        [SwaggerOperation(Summary = "Gọi để chuyển status thành hoàn thành các lessonitem (Student)")]
        public async Task<ActionResult<BaseResponse<bool>>> CompleteLessonItem([FromQuery] int courseKeyId, [FromQuery] int lessonItemId)
        {
            var result = await _courseRegistrationService.CompleteLessonItemAsync(courseKeyId, lessonItemId);
            return Ok(result);
        }

        // Course Content Management endpoints
        [HttpPost("section/create")]
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

        //[HttpGet("{courseId}/sections")]
        //[SwaggerOperation(Summary = "Lấy thông tin của các section (Teacher)")]
        //public async Task<ActionResult<BaseResponse<IEnumerable<CourseSectionResponse>>>> GetCourseSections(long courseId)
        //{
        //    var result = await _courseService.GetCourseSectionsAsync(courseId);
        //    return Ok(result);
        //}

        //[HttpGet("lesson/{lessonId}/items")]
        //[SwaggerOperation(Summary = "Lấy thông tin Lessonitem của Lesson (Teacher)")]
        //public async Task<ActionResult<BaseResponse<IEnumerable<LessonItemResponse>>>> GetLessonItems(long lessonId)
        //{
        //    var result = await _courseService.GetLessonItemsAsync(lessonId);
        //    return Ok(result);
        //}

        [HttpPost("redeem-key")]
        [Authorize(Roles = "Student")]
        [SwaggerOperation(Summary = "Nhập mã key của khóa học để enroll (Student)")]
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

        [HttpGet("get-all-similar-courses")]
        [SwaggerOperation(Summary = "Lấy danh sách các khóa học tương tự")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetAllSimilarCourses([FromBody] SimilarCourseRequest request)
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

        [HttpGet("{courseId}/detail")]
        [SwaggerOperation(Summary = "Lấy tất cả thông tin chi tiết của khóa học (bao gồm module, chapter, sections, lessons, lesson items)")]
        public async Task<ActionResult<BaseResponse<CourseDetailResponse>>> GetCourseDetail(long courseId)
        {
            try
            {
                var result = await _courseService.GetCourseDetailAsync(courseId);
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

        [HttpGet("{courseId}/modules")]
        [SwaggerOperation(Summary = "Xem module của 1 khóa học theo id")]
        public async Task<ActionResult<List<ModuleResponse>>> GetModules(int courseId)
        {
            var result = await _moduleService.GetModulesByCourseIdAsync(courseId);
            return Ok(result);
        }

        [HttpPost("module/create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo các module (Teacher)")]
        public async Task<ActionResult<ModuleResponse>> CreateModule([FromBody] ModuleRequest request)
        {
            var result = await _moduleService.CreateModuleAsync(request);
            return Created("module", result);
        }

        [HttpGet("module/{moduleId}/chapters")]
        [SwaggerOperation(Summary = "Xem chapter của 1 khóa học theo id")]
        public async Task<ActionResult<List<ChapterResponse>>> GetChapters(int moduleId)
        {
            var result = await _chapterService.GetByModuleIdAsync(moduleId);
            return Ok(result);
        }

        [HttpPost("chapter/create")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo các chapter (Teacher)")]
        public async Task<ActionResult<ChapterResponse>> CreateChapter([FromBody] ChapterRequest request)
        {
            var result = await _chapterService.CreateAsync(request);
            return Created("chapter", result);
        }
    }
}
