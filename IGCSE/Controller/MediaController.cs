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

        [HttpGet("get-image")]
        [SwaggerOperation(Summary = "Lấy hình ảnh từ server")]
        public async Task<IActionResult> GetImage([FromQuery] string imagePath)
        {
            return await _mediaService.GetImageOrPdfAsync(_environment.WebRootPath, imagePath);
        }

        [HttpGet("get-video")]
        [SwaggerOperation(Summary = "Lấy video từ server")]
        public async Task<IActionResult> GetVideo([FromQuery] string videoPath)
        {
            var request = HttpContext.Request;
            return await _mediaService.GetVideoAsync(videoPath, request);
        }
    }
}
