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
        [SwaggerOperation(
            Summary = "Lấy hình ảnh, video hoặc PDF từ server", 
            Description = @"Api dùng để lấy file media (hình ảnh, video, PDF) từ server. Sử dụng API `get-media-url` trước để lấy URL, sau đó dùng URL đó để gọi API này.

**Request:**
- Query parameter: `imagePath` (string, required) - Đường dẫn đến file media (lấy từ API `get-media-url`)

**Response:**
- Trả về file media (image/video/pdf) với Content-Type phù hợp
- Status Code: 200 nếu thành công
- Status Code: 404 nếu file không tồn tại

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- `imagePath` phải là đường dẫn hợp lệ từ API `get-media-url`
- Hỗ trợ các định dạng: JPG, PNG, GIF, MP4, PDF
- File được phục vụ trực tiếp từ `wwwroot` folder")]
        public async Task<IActionResult> GetMedia([FromQuery] string imagePath)
        {
            return await _mediaService.GetMediaAsync(_environment.WebRootPath, imagePath);
        }

        [HttpGet("get-media-url")]
        [SwaggerOperation(
            Summary = "Lấy media URL từ server", 
            Description = @"Api dùng để lấy URL đầy đủ của file media (hình ảnh, video, PDF) từ server. URL này có thể dùng trực tiếp để hiển thị media hoặc gọi API `get-media`.

**Request:**
- Query parameter: `imagePath` (string, required) - Đường dẫn tương đối đến file media trong server

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""No answers to mark"",
  ""statusCode"": 200,
  ""data"": ""https://example.com/api/media/get-media?imagePath=courses/image.jpg""
}
```

**Response Schema - Trường hợp lỗi:**

1. **File không tồn tại:**
```json
{
  ""message"": ""File not found"",
  ""statusCode"": 404,
  ""data"": null
}
```

**Lưu ý:**
- API có thể truy cập công khai (không cần đăng nhập)
- `imagePath` là đường dẫn tương đối trong `wwwroot` folder
- URL trả về có thể dùng trực tiếp trong HTML (`<img src>`, `<video src>`, etc.) hoặc gọi API `get-media`
- Hỗ trợ các định dạng: JPG, PNG, GIF, MP4, PDF")]
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
