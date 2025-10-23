namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseAnalyticsResponse
    {
        public int TotalCourse { get; set; }
        public Dictionary<string, int> Partion { get; set; } = new Dictionary<string, int>();
    }
}
