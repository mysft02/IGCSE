namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseReportResponse
    {
        public int CourseReportId { get; set; }

        public int CourseId { get; set; }

        public string UserId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string? ReportType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

