using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.CourseContent
{
    public class CourseSectionRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public long CourseId { get; set; }

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
        public long CourseSectionId { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public sbyte IsActive { get; set; }
    }

    public class LessonItemRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemType { get; set; } = string.Empty; // video, text, quiz, assignment, etc.

        [Required]
        public long LessonId { get; set; }

        [Required]
        public int Order { get; set; }
    }
}
