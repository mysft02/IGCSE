using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace Common.Utils
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; 
        private static readonly string[] AllowedDocumentExtensions = { ".pdf" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".ogg" };
        private const string ImagesFolder = "courses/images";
        private const string AvatarsFolder = "avatar";
        private const string CertificatesFolder = "certificates";
        private const string LessonDocsFolder = "lessons/docs";
        private const string LessonVideosFolder = "lessons/videos";
        private const string CourseCertificatesFolder = "courses/certificates";

        /// <summary>
        /// Uploads an image file and returns the relative URL path
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <param name="webRootPath">Web root path for saving files</param>
        /// <returns>Relative URL path of the uploaded image</returns>
        public static async Task<string> UploadCourseImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedImageExtensions)}");
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
            }

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(webRootPath, ImagesFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL path
            return $"/courses/images/{fileName}";
        }

        public static async Task<string> UploadAvatarAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedImageExtensions)}");
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
            }

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(webRootPath, AvatarsFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL path
            return $"/avatar/{fileName}";
        }

        public static async Task<string> UploadCertificateImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedImageExtensions)}");
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
            }

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(webRootPath, CertificatesFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL path
            return $"/certificates/{fileName}";
        }

        /// <summary>
        /// Uploads a lesson document (e.g., PDF) and returns the relative URL path
        /// </summary>
        public static async Task<string> UploadLessonDocumentAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedDocumentExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedDocumentExtensions)}");
            }

            if (file.Length > 50 * 1024 * 1024) // 50MB for docs
            {
                throw new ArgumentException("File size exceeds 50MB limit");
            }

            var uploadPath = Path.Combine(webRootPath, LessonDocsFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/lessons/docs/{fileName}";
        }

        /// <summary>
        /// Uploads a lesson video and returns the relative URL path
        /// </summary>
        public static async Task<string> UploadLessonVideoAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedVideoExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedVideoExtensions)}");
            }

            if (file.Length > 500 * 1024 * 1024) // 500MB for videos
            {
                throw new ArgumentException("File size exceeds 500MB limit");
            }

            var uploadPath = Path.Combine(webRootPath, LessonVideosFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/lessons/videos/{fileName}";
        }

        /// <summary>
        /// Deletes an image file from the server
        /// </summary>
        /// <param name="imageUrl">Relative URL path of the image to delete</param>
        /// <param name="webRootPath">Web root path</param>
        public static void DeleteCourseImage(string imageUrl, string webRootPath)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(webRootPath, ImagesFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw exception
            }
        }

        /// <summary>
        /// Validates if the file is a valid image
        /// </summary>
        /// <param name="file">File to validate</param>
        /// <returns>True if valid image, false otherwise</returns>
        public static bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedImageExtensions.Contains(fileExtension) && file.Length <= MaxFileSize;
        }

        public static bool IsValidLessonDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedDocumentExtensions.Contains(fileExtension) && file.Length <= 50 * 1024 * 1024;
        }

        public static bool IsValidLessonVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedVideoExtensions.Contains(fileExtension) && file.Length <= 500 * 1024 * 1024;
        }
        
        /// <summary>
        /// Lấy file extension từ Content-Type
        /// </summary>
        public static string GetExtensionFromContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return ".bin";
            }

            return contentType.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "application/pdf" => ".pdf",
                "video/mp4" => ".mp4",
                "video/webm" => ".webm",
                "video/ogg" => ".ogg",
                _ => Path.GetExtension(contentType) ?? ".bin"
            };
        }

        public static async Task<string> GenerateCertificateAsync(string userName, string courseName, string webRootPath)
        {
            var templatePath = Path.Combine(webRootPath, CourseCertificatesFolder, "course-template.png");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Certificate template file not found", templatePath);

            // Tạo thư mục nếu chưa có
            var outputFolder = Path.Combine(webRootPath, CourseCertificatesFolder);
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var fileName = $"{Guid.NewGuid()}.png";
            var outputPath = Path.Combine(outputFolder, fileName);

            File.Copy(templatePath, outputPath, overwrite: true);

            using var bitmap = SKBitmap.Decode(outputPath);
            using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
            var canvas = surface.Canvas;

            canvas.DrawBitmap(bitmap, 0, 0);

            var userPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 54,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Georgia"),
                TextAlign = SKTextAlign.Center
            };

            var coursePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 48,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Georgia"),
                TextAlign = SKTextAlign.Center
            };

            float centerX = bitmap.Width / 2f;

            canvas.DrawText(courseName.ToUpper(), centerX, 835, coursePaint);

            canvas.DrawText(userName.ToUpper(), centerX, 600, userPaint);

            // Export lại PNG
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            await File.WriteAllBytesAsync(outputPath, data.ToArray());

            // Trả về URL cho client
            return $"courses/certificates/{fileName}";
        }
    }
}
