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

        public MediaService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<IActionResult> GetImageOrPdfAsync(string webRootPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new Exception("Thiếu đường dẫn file");

            // Xóa ký tự / hoặc \
            var cleanRelativePath = relativePath.TrimStart('/', '\\');

            var fullPath = Path.Combine(webRootPath, cleanRelativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Không tìm thấy file: {cleanRelativePath}");

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            // Detect content type
            var ext = Path.GetExtension(fullPath).ToLower();
            var contentType = ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",

                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            return new FileStreamResult(fileStream, contentType);
        }

        public async Task<IActionResult> GetVideoAsync(string relativePath, HttpRequest request)
        {
            // relativePath = "images/avatar.png"
            var cleanRelativePath = relativePath.TrimStart('/', '\\');
            var fullPath = Path.Combine(_env.WebRootPath, cleanRelativePath);

            if (!File.Exists(fullPath))
                throw new Exception("Video không tìm thấy");

            var fileInfo = new FileInfo(fullPath);
            long totalSize = fileInfo.Length;

            // Lấy header Range (nếu có)
            request.Headers.TryGetValue("Range", out var rangeHeader);

            // Nếu không có Range → trả full video
            if (string.IsNullOrEmpty(rangeHeader))
            {
                var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(fs, "video/mp4")
                {
                    EnableRangeProcessing = true
                };
            }

            // Parse Range header
            var range = rangeHeader.ToString().Replace("bytes=", "").Split('-');
            long start = long.Parse(range[0]);
            long end = range.Length > 1 && !string.IsNullOrEmpty(range[1])
                        ? long.Parse(range[1])
                        : totalSize - 1;

            long length = end - start + 1;

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(start, SeekOrigin.Begin);

            var response = request.HttpContext.Response;
            response.StatusCode = 206;
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Content-Range", $"bytes {start}-{end}/{totalSize}");
            response.Headers.Add("Content-Length", length.ToString());
            stream.Seek(start, SeekOrigin.Begin);

            var output = new FileStreamResult(stream, "video/mp4");

            return new FileStreamResult(stream, "video/mp4");
        }
    }
}
