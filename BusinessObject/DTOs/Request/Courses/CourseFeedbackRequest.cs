using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseFeedbackRequest
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
    }
}

