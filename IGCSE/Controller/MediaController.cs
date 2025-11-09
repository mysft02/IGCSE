using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly MediaService _mediaService;
        private readonly IWebHostEnvironment _environment;

        public MediaController(MediaService mediaService, IWebHostEnvironment environment)
        {
            _mediaService = mediaService;
            _environment = environment;
        }

        [HttpGet("get-media")]
        [SwaggerOperation(Summary = "Lấy hình ảnh, video hoặc pdf từ server")]
        public async Task<IActionResult> GetMedia([FromQuery] string imagePath)
        {
            return await _mediaService.GetMediaAsync(_environment.WebRootPath, imagePath);
        }
    }
}
