using DTOs.Request.Courses;
using DTOs.Response.Courses;
using DTOs.Request.CourseRegistration;
using DTOs.Response.CourseRegistration;
using DTOs.Response.CourseContent;
using DTOs.Request.CourseContent;
using Microsoft.AspNetCore.Mvc;
using Service;
using DTOs.Response.Accounts;

namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly CourseRegistrationService _courseRegistrationService;

        public CourseController(CourseService courseService, CourseRegistrationService courseRegistrationService)
        {
            _courseService = courseService;
            _courseRegistrationService = courseRegistrationService;
        }

        // Existing course management endpoints
        [HttpPost("create")]
        public async Task<ActionResult<BaseResponse<CourseResponse>>> CreateCourse([FromBody] CourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                var result = await _courseService.CreateCourseAsync(request);
                return CreatedAtAction(nameof(GetCourse), new { id = result.Data.CourseId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CourseResponse>>> UpdateCourse(int id, [FromBody] CourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                var result = await _courseService.UpdateCourseAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CourseResponse>>> GetCourse(int id)
        {
            try
            {
                var result = await _courseService.GetCourseByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.NotFound_404,
                    null
                ));
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<IEnumerable<CourseResponse>>>> GetAllCourses()
        {
            try
            {
                var result = await _courseService.GetAllCoursesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        // Course Registration endpoints
        [HttpPost("register")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CourseRegistrationResponse>>> RegisterForCourse([FromBody] CourseRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
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
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("registrations/{studentId}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<IEnumerable<CourseRegistrationResponse>>>> GetStudentRegistrations(string studentId)
        {
            try
            {
                var result = await _courseRegistrationService.GetStudentRegistrationsAsync(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("content/{courseKeyId}/section/{courseSectionId}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CourseSectionResponse>>> GetCourseContent(long courseKeyId, long courseSectionId)
        {
            try
            {
                var result = await _courseRegistrationService.GetCourseContentAsync(courseKeyId, courseSectionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.NotFound_404,
                    null
                ));
            }
        }

        [HttpGet("progress/{courseKeyId}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<StudentProgressResponse>>> GetStudentProgress(long courseKeyId)
        {
            try
            {
                var result = await _courseRegistrationService.GetStudentProgressAsync(courseKeyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.NotFound_404,
                    null
                ));
            }
        }

        [HttpPost("complete-lesson-item")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<bool>>> CompleteLessonItem([FromQuery] int courseKeyId, [FromQuery] int lessonItemId)
        {
            try
            {
                var result = await _courseRegistrationService.CompleteLessonItemAsync(courseKeyId, lessonItemId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        // Course Content Management endpoints
        [HttpPost("section/create")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CourseSectionResponse>>> CreateCourseSection([FromBody] CourseSectionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
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
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("lesson/create")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<LessonResponse>>> CreateLesson([FromBody] LessonRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
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
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPost("lesson-item/create")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<LessonItemResponse>>> CreateLessonItem([FromBody] LessonItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
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
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("{courseId}/sections")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<IEnumerable<CourseSectionResponse>>>> GetCourseSections(long courseId)
        {
            try
            {
                var result = await _courseService.GetCourseSectionsAsync(courseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("lesson/{lessonId}/items")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<IEnumerable<LessonItemResponse>>>> GetLessonItems(long lessonId)
        {
            try
            {
                var result = await _courseService.GetLessonItemsAsync(lessonId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}
