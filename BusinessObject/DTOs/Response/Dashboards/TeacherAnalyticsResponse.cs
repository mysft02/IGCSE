namespace BusinessObject.DTOs.Response.Dashboards
{
    public class TeacherAnalyticsResponse
    {
        public CourseCountResponse Course { get; set; }

        public IncomeAnalyticsResponse Income { get; set; }
    }

    public class CourseCountResponse
    {
        public int TotalCourses { get; set; }
        
        public int Pending { get; set; }

        public int Open {  get; set; }

        public int Rejected { get; set; }

        public int Draft { get; set; }
    }

    public class IncomeAnalyticsResponse
    {
        public List<MonthlyIncomeCount> IncomeCount { get; set; }
    }

    public class MonthlyIncomeCount
    {
        public int Month { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal ReceivedIncome { get; set; }
    }
}
