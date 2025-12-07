using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using BusinessObject.Model;
using Microsoft.AspNetCore.Authorization;
using Common.Utils;
using BusinessObject.DTOs.Request.Packages;
using BusinessObject.DTOs.Response.Packages;
using System.Security.Claims;

namespace IGCSE.Controller
{
    [Route("api/package")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly PackageService _packageService;

        public PackageController(PackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet("get-all-package")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy toàn bộ package (có phân trang và filter theo role)", 
            Description = @"Api dùng để lấy danh sách package trong hệ thống với phân trang. Hệ thống tự động filter theo role của user.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `PackageQueryRequest`

**Logic tự động theo role:**
- Nếu role là `Parent` hoặc `Student`: Mặc định lấy package với `isMockTest = true` (gói để mở khóa mock test)
- Nếu role là `Teacher`: Mặc định lấy package với `isMockTest = false` (gói để tạo thêm course)
- Cột `slot` chỉ có tác dụng với gói dành cho giáo viên, thể hiện số lượng course có thể tạo thêm sau khi mua gói

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy toàn bộ package thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""packageId"": 1,
        ""title"": ""Package Title"",
        ""description"": ""Package Description"",
        ""isActive"": true,
        ""isMockTest"": true,
        ""price"": 100000,
        ""slot"": 0
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

1. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID và Role được lấy tự động từ JWT token
- Kết quả được filter tự động theo role của user")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<PackageQueryResponse>>>> GetAllPackages([FromQuery] PackageQueryRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            request.Role = userRole;
            request.userID = userId;

            var result = await _packageService.GetAllPackagesAsync(request);
            return Ok(result);
        }

        [HttpGet("get-package-by-id")]
        [SwaggerOperation(
            Summary = "Lấy thông tin chi tiết package theo id", 
            Description = @"Api dùng để lấy thông tin chi tiết của một package theo ID.

**Request:**
- Query parameter: `id` (int, required) - ID của package cần lấy

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy package thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""packageId"": 1,
    ""title"": ""Package Title"",
    ""description"": ""Package Description"",
    ""isActive"": true,
    ""isMockTest"": true,
    ""price"": 100000,
    ""slot"": 0
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Package không tồn tại:**
```json
{
  ""message"": ""Không tìm thấy package"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- Trả về thông tin chi tiết của package")]
        public async Task<ActionResult<BaseResponse<Package>>> GetPackageById([FromQuery] int id)
        {
            var result = await _packageService.GetPackageByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("get-owned-package")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Lấy toàn bộ package người dùng đã mua (có phân trang)", 
            Description = @"Api dùng để lấy danh sách tất cả package mà user đã mua với phân trang.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 0) - Số trang (0-based)
  - `Size` (int, mặc định: 10) - Số lượng item mỗi trang
  - Các query parameters khác tùy theo `PackageOwnedQueryRequest`

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy package đã mua thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""packageId"": 1,
        ""title"": ""Package Title"",
        ""description"": ""Package Description"",
        ""price"": 100000,
        ""slot"": 0,
        ""isMockTest"": true,
        ""buyDate"": ""2024-01-15T10:30:00Z"",
        ""buyPrice"": 100000
      }
    ],
    ""totalCount"": 5,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 1
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```

**Lưu ý:**
- API yêu cầu đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Chỉ trả về các package mà user đã mua")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<PackageOwnedQueryResponse>>>> GetOwnedPackage([FromQuery] PackageOwnedQueryRequest request)
        {
            var user = HttpContext.User;
            var userId = user.FindFirst("AccountID")?.Value;

            if (CommonUtils.IsEmptyString(userId))
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            request.userID = userId;

            var result = await _packageService.GetOwnedPackageAsync(request);
            return Ok(result);
        }

        [HttpPost("create-package")]
        [SwaggerOperation(Summary = "Tạo mới package")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<Package>>>> CreatePackage([FromForm] PackageCreateRequest request)
        {
            var result = await _packageService.CreatePackageAsync(request);
            return Ok(result);
        }
        
        [HttpPost("update-package")]
        [SwaggerOperation(Summary = "Cập nhật package")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<Package>>>> UpdatePackage([FromForm] PackageUpdateRequest request)
        {
            var result = await _packageService.UpdatePackageAsync(request);
            return Ok(result);
        }
    }
}
