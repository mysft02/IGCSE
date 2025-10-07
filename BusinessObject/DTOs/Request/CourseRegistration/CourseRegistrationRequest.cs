using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.CourseRegistration
{
    public class CourseRegistrationRequest
    {
        [Required]
        public long CourseId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
    }
}
