using DTOs.Request.Courses;
using DTOs.Response.Courses;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;

        public CourseController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CourseResponse>>> CreateCourse([FromBody] CourseRequest request)
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
                return CreatedAtAction(nameof(GetCourse), new { id = result.Data.CourseID }, result);
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
    }
}
