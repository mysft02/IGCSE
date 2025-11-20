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
        [SwaggerOperation(Summary = "Lấy toàn bộ package (có phân trang)", Description = "Dùng để lấy toàn bộ package trong hệ thống. " +
            "Nếu role là `parent` hoặc `student` thì sẽ mặc định lấy toàn bộ package dùng để mở khoá mocktest với cột `isMockTest` là `true`. " +
            "Nếu role là `teacher` thì sẽ mặc định lấy toàn bộ package dùng để tạo thêm course với cột `isMockTest` là `false`. " +
            "Cột `slot` chỉ có tác dụng với gói dành cho giáo viên thể hiện cho số lượng course có thể tạo thêm sau khi mua gói.")]
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
        [SwaggerOperation(Summary = "Lấy 1 package theo id")]
        public async Task<ActionResult<BaseResponse<Package>>> GetPackageById([FromQuery] int id)
        {
            var result = await _packageService.GetPackageByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("get-owned-package")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy toàn bộ package người dùng đã mua")]
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
