namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseFeedbackResponse
    {
        public int CourseFeedbackId { get; set; }

        public int CourseId { get; set; }

        public string StudentId { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

