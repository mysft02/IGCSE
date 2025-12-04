namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseCertificateResponse
    {
        public int CourseId { get; set; }

        public string UserId { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
