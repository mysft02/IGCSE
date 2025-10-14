namespace DTOs.Response.Courses
{
    public class CourseKeyResponse
    {
        public string KeyValue { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int? StudentId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
