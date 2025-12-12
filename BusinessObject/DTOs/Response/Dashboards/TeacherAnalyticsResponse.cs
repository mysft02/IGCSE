namespace BusinessObject.DTOs.Response.Dashboards
{
    /// <summary>
    /// Response cho Teacher Analytics (sử dụng pattern chung)
    /// </summary>
    public class TeacherAnalyticsResponse : BaseAnalyticsResponse<TeacherAnalyticsMetadata, IncomeChartDataPoint>
    {
    }

    // Keep old classes for backward compatibility (deprecated)
    [Obsolete("Use TeacherAnalyticsResponse with BaseAnalyticsResponse pattern instead")]
    public class CourseCountResponse
    {
        public int TotalCourses { get; set; }
        
        public int Pending { get; set; }

        public int Open {  get; set; }

        public int Rejected { get; set; }

        public int Draft { get; set; }
    }

    [Obsolete("Use IncomeChartDataPoint instead")]
    public class IncomeAnalyticsResponse
    {
        public List<MonthlyIncomeCount> IncomeCount { get; set; }
    }

    [Obsolete("Use IncomeChartDataPoint instead")]
    public class MonthlyIncomeCount
    {
        public int Month { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal ReceivedIncome { get; set; }
    }
}
