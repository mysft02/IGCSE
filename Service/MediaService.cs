using BusinessObject.DTOs.Response;
using Common.Constants;
using Microsoft.AspNetCore.Hosting;

namespace Service
{
    public class MediaService
    {
        private readonly IWebHostEnvironment _env;

        public MediaService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<BaseResponse<string>> GetImageAsync(string webRootPath,string relativePath)
        {
            // relativePath = "images/avatar.png"
            var cleanRelativePath = relativePath.TrimStart('/', '\\');
            var fullPath = Path.Combine(webRootPath, cleanRelativePath);

            if (!File.Exists(fullPath))
                throw new Exception("Không tìm thấy hình ảnh");

            var bytes = await File.ReadAllBytesAsync(fullPath);

            var response = Convert.ToBase64String(bytes);

            return new BaseResponse<string>
            (
                "Lấy hình ảnh thành công",
                StatusCodeEnum.OK_200,
                response
            );
        }
    }
}
