using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.CourseRegistration
{
    public class CourseRegistrationRequest
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
    }
}
