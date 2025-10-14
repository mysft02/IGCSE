using DTOs.Request.Courses;
using DTOs.Response.Courses;
using DTOs.Request.CourseRegistration;
using DTOs.Response.CourseRegistration;
using DTOs.Response.CourseContent;
using DTOs.Request.CourseContent;
using Microsoft.AspNetCore.Mvc;
using Service;
using DTOs.Response.Accounts;
using Common.Utils;
using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using Microsoft.AspNetCore.Http.Extensions;

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

        public CourseController(CourseService courseService, CourseRegistrationService courseRegistrationService, IWebHostEnvironment webHostEnvironment, PaymentService paymentService)
        {
            _courseService = courseService;
            _courseRegistrationService = courseRegistrationService;
            _webHostEnvironment = webHostEnvironment;
            _paymentService = paymentService;
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

            if (request.ImageFile != null && FileUploadHelper.IsValidImageFile(request.ImageFile))
            {
                request.ImageUrl = await FileUploadHelper.UploadCourseImageAsync(request.ImageFile, _webHostEnvironment.WebRootPath);
            }

            var result = await _courseService.CreateCourseAsync(request);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetAllCourses()
        {
            var result = await _courseService.GetAllCoursesAsync();
            return Ok(result);
        }

        // Course Registration endpoints
        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<CourseRegistrationResponse>>> RegisterForCourse([FromBody] CourseRegistrationRequest request)
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

            var result = await _courseRegistrationService.RegisterForCourseAsync(request);
            return Created("course registration", result);
        }

        [HttpGet("registrations/{studentId}")]
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

        [HttpGet("progress/{courseKeyId}")]
        public async Task<ActionResult<BaseResponse<StudentProgressResponse>>> GetStudentProgress(long courseKeyId)
        {
            var result = await _courseRegistrationService.GetStudentProgressAsync(courseKeyId);
            return Ok(result);
        }

        [HttpPost("complete-lesson-item")]
        public async Task<ActionResult<BaseResponse<bool>>> CompleteLessonItem([FromQuery] int courseKeyId, [FromQuery] int lessonItemId)
        {
            var result = await _courseRegistrationService.CompleteLessonItemAsync(courseKeyId, lessonItemId);
            return Ok(result);
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

            var result = await _courseService.CreateCourseSectionAsync(request);
            return Created("course section", result);
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

            var result = await _courseService.CreateLessonAsync(request);
            return Created("lesson", result);
        }

        [HttpPost("lesson-item/create")]
        public async Task<ActionResult<BaseResponse<LessonItemResponse>>> CreateLessonItem([FromBody] LessonItemRequest request)
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

            var result = await _courseService.CreateLessonItemAsync(request);
            return Created("lesson item", result);
        }

        [HttpGet("{courseId}/sections")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseSectionResponse>>>> GetCourseSections(long courseId)
        {
            var result = await _courseService.GetCourseSectionsAsync(courseId);
            return Ok(result);
        }

        [HttpGet("lesson/{lessonId}/items")]
        public async Task<ActionResult<BaseResponse<IEnumerable<LessonItemResponse>>>> GetLessonItems(long lessonId)
        {
            var result = await _courseService.GetLessonItemsAsync(lessonId);
            return Ok(result);
        }

        [HttpPost("redeem-key")]
        public async Task<ActionResult<BaseResponse<string>>> RedeemCourseKey([FromBody] string courseKeyValue)
        {
            // Lấy user hiện tại từ claims
            var user = HttpContext.User;
            var studentId = user.FindFirst("AccountID")?.Value;
            var roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList();
            if (string.IsNullOrEmpty(studentId) || !roles.Contains("Student"))
            {
                return Forbid("Chỉ tài khoản học sinh mới được sử dụng mã khoá học!");
            }
            // Tìm mã khoá theo KeyValue
            var courseKeys = await _courseRegistrationService.GetAllCourseKeysAsync();
            var keyObj = courseKeys.FirstOrDefault(x => x.Status == "available" && x.StudentId == null && x.KeyValue == courseKeyValue);
            if (keyObj == null)
            {
                return NotFound(new BaseResponse<string>("Mã khoá không tồn tại, đã được sử dụng, hoặc không hợp lệ.", Common.Constants.StatusCodeEnum.NotFound_404, null));
            }
            // Gán cho Student, đổi status
            keyObj.StudentId = studentId;
            keyObj.Status = "redeemed";
            keyObj.UpdatedAt = DateTime.UtcNow;
            await _courseRegistrationService.UpdateCourseKeyAsync(keyObj);
            // Khởi tạo progress cho học sinh
            await _courseRegistrationService.InitializeCourseProgressAsync(keyObj.CourseKeyId);
            return Ok(new BaseResponse<string>("Kích hoạt khoá học thành công!", Common.Constants.StatusCodeEnum.OK_200, keyObj.KeyValue));
        }

        [HttpPost("create-vnpay-url")]
        public async Task<ActionResult<BaseResponse<PaymentResponse>>> CreatePaymentUrl([FromBody] PaymentRequest request)
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

            var result = await _paymentService.CreatePaymentUrlAsync(HttpContext, request);
            return Ok(result);
        }

        [HttpPost("vnpay-callback")]
        public async Task<ActionResult<BaseResponse<string>>> VnPayCallback()
        {
            var currentUrl = Request.GetDisplayUrl();

            var queryParams = Request.Query
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            var result = await _paymentService.HandlePaymentSuccessAsync(queryParams);
            return Ok(result);
        }
    }
}
