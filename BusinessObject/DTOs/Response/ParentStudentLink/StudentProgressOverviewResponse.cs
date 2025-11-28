namespace BusinessObject.DTOs.Response.ParentStudentLink
{
    public class StudentProgressOverviewResponse
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public List<CourseProgressSummary> Courses { get; set; } = new();
    }

    public class CourseProgressSummary
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseImageUrl { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public double OverallProgress { get; set; } // 0-100%
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
    }
}

