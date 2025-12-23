using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseReportRequest
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        /// <summary>
        /// Loại report: ví dụ \"nội dung không phù hợp\", \"trùng lặp\"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = null!;
    }
}

