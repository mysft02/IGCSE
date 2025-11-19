namespace BusinessObject.DTOs.Response.ParentStudentLink
{
    public class ParentEnrollmentResponse
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseDescription { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }
    }
}


