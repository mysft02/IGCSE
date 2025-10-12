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
        public async Task<ActionResult<BaseResponse<IEnumerable<CourseResponse>>>> GetAllCourses()
        {
            try
            {
                var result = await _courseService.GetAllCoursesAsync();
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

            try
            {
                var result = await _courseRegistrationService.RegisterForCourseAsync(request);
                return Created("course registration", result);
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

        [HttpGet("progress/{courseKeyId}")]
        public async Task<ActionResult<BaseResponse<StudentProgressResponse>>> GetStudentProgress(long courseKeyId)
        {
            try
            {
                var result = await _courseRegistrationService.GetStudentProgressAsync(courseKeyId);
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

            try
            {
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
    }
}
