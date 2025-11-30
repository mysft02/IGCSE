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
        [SwaggerOperation(
            Summary = "Lấy danh sách tất cả module (có paging và filter)", 
            Description = @"Api dùng để lấy danh sách tất cả module trong hệ thống với phân trang và bộ lọc.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 1) - Số trang
  - `PageSize` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `ModuleListQuery`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Modules retrieved successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""moduleId"": 1,
        ""moduleName"": ""Module Name"",
        ""description"": ""Mô tả module"",
        ""isActive"": true,
        ""createdAt"": ""2024-01-01T00:00:00Z"",
        ""updatedAt"": ""2024-01-01T00:00:00Z"",
        ""courseSubject"": ""Math""
      }
    ],
    ""totalCount"": 20,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 2
  }
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Có lỗi xảy ra khi lấy danh sách module"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Kết quả được phân trang để tối ưu hiệu suất")]
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
        [SwaggerOperation(
            Summary = "Lấy danh sách module theo môn học", 
            Description = @"Api dùng để lấy danh sách module theo môn học cụ thể.

**Request:**
- Path parameter: `courseSubject` (enum, required) - Môn học: `Math`, `Physics`, `Chemistry`, `Biology`, `English`, etc.

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Modules retrieved successfully"",
  ""statusCode"": 200,
  ""data"": [
    {
      ""moduleId"": 1,
      ""moduleName"": ""Module Name"",
      ""description"": ""Mô tả module"",
      ""isActive"": true,
      ""createdAt"": ""2024-01-01T00:00:00Z"",
      ""updatedAt"": ""2024-01-01T00:00:00Z"",
      ""courseSubject"": ""Math""
    }
  ]
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Có lỗi xảy ra khi lấy danh sách module theo môn học"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Chỉ trả về các module có `isActive = true`")]
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
        [SwaggerOperation(
            Summary = "Tạo mới module", 
            Description = @"Api dùng để tạo module mới trong hệ thống.

**Request:**
- Body:
```json
{
  ""moduleName"": ""Module Name"",
  ""description"": ""Mô tả module"",
  ""courseSubject"": ""Math"",
  ""isActive"": true
}
```

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Module created successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""moduleId"": 1,
    ""moduleName"": ""Module Name"",
    ""description"": ""Mô tả module"",
    ""isActive"": true,
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-01T00:00:00Z"",
    ""courseSubject"": ""Math""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Invalid request data"",
  ""statusCode"": 400,
  ""data"": [""Error details""]
}
```

2. **Lỗi server:**
```json
{
  ""message"": ""Có lỗi xảy ra khi tạo module"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Tất cả các trường đều được validate trước khi tạo")]
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
        [SwaggerOperation(
            Summary = "Cập nhật thông tin module", 
            Description = @"Api dùng để cập nhật thông tin của một module đã tồn tại.

**Request:**
- Path parameter: `moduleId` (int, required) - ID của module cần cập nhật
- Body:
```json
{
  ""moduleName"": ""Updated Module Name"",
  ""description"": ""Updated description"",
  ""courseSubject"": ""Math"",
  ""isActive"": true
}
```

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Module updated successfully"",
  ""statusCode"": 200,
  ""data"": {
    ""moduleId"": 1,
    ""moduleName"": ""Updated Module Name"",
    ""description"": ""Updated description"",
    ""isActive"": true,
    ""createdAt"": ""2024-01-01T00:00:00Z"",
    ""updatedAt"": ""2024-01-15T10:30:00Z"",
    ""courseSubject"": ""Math""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Module không tồn tại:**
```json
{
  ""message"": ""Module not found"",
  ""statusCode"": 404,
  ""data"": null
}
```

2. **Dữ liệu không hợp lệ:**
```json
{
  ""message"": ""Invalid request data"",
  ""statusCode"": 400,
  ""data"": [""Error details""]
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Tất cả các trường đều được validate trước khi cập nhật")]
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
        [SwaggerOperation(
            Summary = "Xóa module", 
            Description = @"Api dùng để xóa một module khỏi hệ thống.

**Request:**
- Path parameter: `moduleId` (int, required) - ID của module cần xóa

**Response:**
- Status Code: 204 (No Content) nếu thành công

**Response Schema - Trường hợp lỗi:**

1. **Module không tồn tại:**
```json
{
  ""message"": ""Không tìm thấy module"",
  ""statusCode"": 404
}
```

2. **Lỗi server:**
```json
{
  ""message"": ""Có lỗi xảy ra khi xóa module 1"",
  ""statusCode"": 500
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Xóa module có thể ảnh hưởng đến các khóa học liên quan
- Nên kiểm tra các ràng buộc trước khi xóa")]
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


