using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.CourseContent
{
    public class CourseSectionRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        //[Required]
        //public int ChapterId { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public sbyte IsActive { get; set; }
    }

    public class LessonRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CourseSectionId { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public sbyte IsActive { get; set; }
    }

    public class LessonItemRequest
    {
        [Required]
        [StringLength(200)]
        [SwaggerSchema("Tên thành phần bài học", Nullable = false)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [SwaggerSchema("Mô tả thành phần bài học", Nullable = false)]
        public string? Description { get; set; }

        [StringLength(2000)]
        [SwaggerSchema("Nội dung thành phần bài học", Nullable = false)]
        public string? Content { get; set; }

        [Required]
        [StringLength(50)]
        [SwaggerSchema("Thể loại thành phần bài học", Nullable = false, Description = "video, bài giảng, bài kiểm tra,...")]
        public string ItemType { get; set; } = string.Empty; // video, text, quiz, assignment, etc.

        [Required]
        [SwaggerSchema("Id bài học của item này", Nullable = false)]
        public int LessonId { get; set; }

        [Required]
        [SwaggerSchema("Thứ tự thành phần bài học", Nullable = false)]
        public int Order { get; set; }

        // Optional file to upload for this lesson item (pdf/video)
        [SwaggerSchema("File thành phần bài học", Description = "Tải file pdf hoặc video lên")]
        public IFormFile? File { get; set; }
    }
}
