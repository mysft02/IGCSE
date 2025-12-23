using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseReportStatusUpdateRequest
    {
        /// <summary>
        /// Trạng thái mới: PROCESSING hoặc COMPLETED
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = null!;
    }
}

