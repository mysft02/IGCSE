using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class DisableCourseRequest
    {
        /// <summary>
        /// Lý do ẩn khóa học (sẽ gửi cho Teacher)
        /// </summary>
        [Required(ErrorMessage = "Lý do ẩn khóa học là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Lý do không được vượt quá 1000 ký tự")]
        public string Reason { get; set; } = null!;
    }
}
