using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Utils
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private const string ImagesFolder = "wwwroot/images/courses";

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
    }
}
