using Microsoft.AspNetCore.Http;

namespace Common.Utils
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; 
        private static readonly string[] AllowedDocumentExtensions = { ".pdf" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".ogg" };
        private const string ImagesFolder = "wwwroot/images/courses";
        private const string LessonDocsFolder = "wwwroot/lessons/docs";
        private const string LessonVideosFolder = "wwwroot/lessons/videos";

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
            return $"/images/courses/{fileName}";
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
    }
}
