using BusinessObject.DTOs.Request.Certificates;
using BusinessObject.DTOs.Request.TeacherProfiles;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.TeacherProfile;
using Common.Constants;
using Common.Utils;
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

        [HttpPost("create-or-update-teacher-profile")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Tạo hoặc cập nhật profile cho teacher")]
        public async Task<ActionResult<BaseResponse<TeacherProfileResponse>>> CreateTeacherProfile([FromQuery] TeacherProfileCreateRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _teacherProfileService.CreateTeacherProfile(request, userId);
            return Ok(result);
        }

        [HttpPost("create-certificate")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Thêm bằng cấp vào profile của giáo viên")]
        public async Task<ActionResult<BaseResponse<CertificateResponse>>> UploadCertificate([FromQuery] CertificateCreateRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _teacherProfileService.UploadCertificate(request, userId);
            return Ok(result);
        }

        [HttpPost("add-payment-information")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(Summary = "Thêm thông tin thanh toán của giáo viên")]
        public async Task<ActionResult<BaseResponse<CertificateResponse>>> AddPaymentInfo([FromQuery] CertificateCreateRequest request)
        {
            var userId = HttpContext.User.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var result = await _teacherProfileService.UploadCertificate(request, userId);
            return Ok(result);
        }
    }
}
