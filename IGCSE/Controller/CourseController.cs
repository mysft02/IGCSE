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
        //private readonly ChapterService _chapterService;
        private readonly MediaService _mediaService;
        private readonly IWebHostEnvironment _environment;
        private readonly PaymentService _paymentService;

        public CourseController(
            CourseService courseService,
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
            _paymentService = paymentService;
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
                    StatusCodeEnum.BadRequest_400,
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
        [SwaggerOperation(Summary = "Lấy danh sách khóa học đã đăng ký của chính mình (Student)")]
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
        [SwaggerOperation(Summary = "Lấy tất cả khóa học do teacher đã tạo (Teacher)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<CourseResponse>>>> GetMyCreatedCourses([FromQuery] TeacherCourseQueryRequest request)
        {
            var user = HttpContext.User;
            var teacherId = user.FindFirst("AccountID")?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
            }

            var result = await _courseService.GetTeacherCoursesAsync(teacherId, request);
            return Ok(result);
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
                    StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            var result = await _courseService.GetAllSimilarCoursesAsync(request.CourseId, request.Score);
            return Ok(result);
        }

        [HttpGet("{courseId}")]
        [SwaggerOperation(Summary = "Lấy tất cả thông tin chi tiết của khóa học (bao gồm sections, lessons, lesson items và tiến trình học nếu đã đăng nhập và enroll)")]
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
                    var roles = User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
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
        [SwaggerOperation(Summary = "Lấy tất cả thông tin chi tiết của bài học")]
        public async Task<ActionResult<BaseResponse<LessonDetailResponse>>> GetLessonItemDetail([FromQuery]int lessonItemId)
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
        [SwaggerOperation(Summary = "Xem tiến trình học của các học sinh đã liên kết với Parent (Parent)")]
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
        [SwaggerOperation(Summary = "Xem danh sách khóa học Parent đã mua (Parent)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<ParentEnrollmentResponse>>>> GetCourseBuyByParent([FromQuery] ParentEnrollmentQueryRequest request)
        {
            try
            {
                var user = HttpContext.User;
                var parentId = user.FindFirst("AccountID")?.Value;

                if (string.IsNullOrEmpty(parentId))
                {
                    return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
                }

                var result = await _courseService.GetCourseBuyByParentAsync(parentId, request);
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
