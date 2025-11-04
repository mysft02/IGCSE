using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;
using BusinessObject.Model;
using BusinessObject.Payload.Request;

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
        [SwaggerOperation(Summary = "Lấy toàn bộ package (có phân trang)")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<Package>>>> GetAllPackages([FromQuery] PackageQueryRequest request)
        {
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
