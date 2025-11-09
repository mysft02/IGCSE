using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service
{
    public class MediaService
    {
        private readonly IWebHostEnvironment _env;

        public MediaService(IWebHostEnvironment env)
        {
            _env = env;
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
    }
}
