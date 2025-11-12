using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Response.Modules;
using Swashbuckle.AspNetCore.Annotations;
using Service;
using Common.Constants;
using BusinessObject.DTOs.Response;
using BusinessObject.Enums;

namespace IGCSE.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly ModuleService _moduleService;

        public ModuleController(ModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lấy danh sách tất cả module")]
        public async Task<IActionResult> GetAllModules([FromQuery] ModuleListQuery query)
        {
            try
            {
                var response = await _moduleService.GetModulesPagedAsync(query);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<object>(
                    "Có lỗi xảy ra khi lấy danh sách module",
                    StatusCodeEnum.InternalServerError_500,
                    null
                ));
            }
        }

        [HttpGet("subject/{courseSubject}")]
        [SwaggerOperation(Summary = "Lấy module theo môn học")]
        public async Task<IActionResult> GetBySubject([FromRoute] CourseSubject courseSubject)
        {
            try
            {
                var response = await _moduleService.GetModulesBySubjectAsync(courseSubject);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<object>(
                    "Có lỗi xảy ra khi lấy danh sách module theo môn học",
                    StatusCodeEnum.InternalServerError_500,
                    null
                ));
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Tạo mới module")]
        public async Task<IActionResult> CreateModule([FromBody] ModuleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<object>(
                        "Invalid request data",
                        StatusCodeEnum.BadRequest_400,
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    ));
                }

                var response = await _moduleService.CreateModuleAsync(request);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<object>(
                    "Có lỗi xảy ra khi tạo module",
                    StatusCodeEnum.InternalServerError_500,
                    null
                ));
            }
        }


        [HttpPut("{moduleId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin module")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] ModuleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<object>(
                        "Invalid request data",
                        StatusCodeEnum.BadRequest_400,
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    ));
                }

                var response = await _moduleService.UpdateModuleAsync(moduleId, request);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<object>(
                    $"Có lỗi xảy ra khi cập nhật module {moduleId}",
                    StatusCodeEnum.InternalServerError_500,
                    null
                ));
            }
        }

        [HttpDelete("{moduleId}")]
        [SwaggerOperation(Summary = "Xóa module")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            try
            {
                await _moduleService.DeleteModuleAsync(moduleId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message == "Module not found")
            {
                return NotFound("Không tìm thấy module");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Có lỗi xảy ra khi xóa module {moduleId}");
            }
        }
    }
}


