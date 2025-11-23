using BusinessObject.DTOs.Response;
using Common.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service
{
    public class MediaService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MediaService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> GetMediaAsync(string webRootPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new Exception("Thiếu đường dẫn file");

            // Xóa ký tự / hoặc \
            var cleanRelativePath = relativePath.TrimStart('/', '\\');

            var fullPath = Path.Combine(webRootPath, cleanRelativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Không tìm thấy file: {cleanRelativePath}");

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            var ext = Path.GetExtension(fullPath).ToLower();

            var contentType = ext switch
            {
                // --- IMAGE ---
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",

                // --- PDF ---
                ".pdf" => "application/pdf",

                // --- VIDEO ---
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                ".webm" => "video/webm",

                _ => "application/octet-stream"
            };

            return new FileStreamResult(fileStream, contentType);
        }

        public async Task<string> GetMediaUrlAsync(string relativePath)
        {
            var webRootPath = _env.WebRootPath ?? _env.ContentRootPath;

            // loại bỏ / hoặc \
            var cleanRelativePath = relativePath.TrimStart('/', '\\');

            var request = _httpContextAccessor.HttpContext?.Request;

            // ✅ Encode relativePath để Swagger hiển thị đúng images%2F...
            var encodedPath = Uri.EscapeDataString(cleanRelativePath);

            var result = $"{request.Scheme}://{request.Host.Value}/api/media/get-media?imagePath={encodedPath}";

            return result;
        }
    }
}
