using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.TeacherProfile;
using BusinessObject.Model;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace IGCSE.Controller
{
    [Route("api/teacher-profile")]
    [ApiController]
    public class TeacherProfileController : ControllerBase
    {
        private readonly TeacherProfileService _teacherProfileService;

        public TeacherProfileController(TeacherProfileService teacherProfileService)
        {
            _teacherProfileService = teacherProfileService;
        }

        [HttpGet("get-teacher-profile-by-id")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy hồ sơ theo id của giáo viên")]
        public async Task<ActionResult<BaseResponse<TeacherProfileResponse>>> GetProfileById([FromQuery] string? id)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse<string>("Không xác định được tài khoản.", StatusCodeEnum.Unauthorized_401, null));
            }

            var teacherId = userId;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Manager")
            {
                teacherId = id;
            }

            var result = await _teacherProfileService.GetProfileByIdAsync(teacherId);
            return Ok(result);
        }
    }
}
