namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseCertificatesQuery
    {
        public int Page { get; set; } = 0;

        public int Size { get; set; } = 10;

        public string? StudentId { get; set; }
    }
}
