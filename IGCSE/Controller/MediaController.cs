using BusinessObject.DTOs.Response;
using Common.Constants;
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
        [SwaggerOperation(Summary = "Lấy hình ảnh, video hoặc pdf từ server", Description = "Sử dụng api `get-media-url` trước để lấy url hình ảnh rồi dùng url đó để gọi api này")]
        public async Task<IActionResult> GetMedia([FromQuery] string imagePath)
        {
            return await _mediaService.GetMediaAsync(_environment.WebRootPath, imagePath);
        }

        [HttpGet("get-media-url")]
        [SwaggerOperation(Summary = "Lấy media url từ server", Description = "Sử dụng api này để lấy đường dẫn hình ảnh, video hoặc pdf rồi dùng trực tiếp đường dẫn đó để lấy media")]
        public async Task<BaseResponse<string>> GetMediaUrl([FromQuery] string imagePath)
        {
            var result = await _mediaService.GetMediaUrlAsync(imagePath);

            return new BaseResponse<string>
            {
                Data = result,
                Message = "No answers to mark",
                StatusCode = StatusCodeEnum.OK_200,
            };
        }
    }
}
