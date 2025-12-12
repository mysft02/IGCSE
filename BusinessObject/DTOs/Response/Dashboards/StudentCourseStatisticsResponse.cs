namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Course Statistics API
/// </summary>
public class StudentCourseStatisticsResponse : BaseAnalyticsResponse<StudentCourseStatisticsMetadata, StudentCourseStatisticsChartDataPoint>
{
}

/// <summary>
/// Metadata cho Course Statistics
/// </summary>
public class StudentCourseStatisticsMetadata
{
    /// <summary>
    /// Tổng số course đã đăng ký
    /// </summary>
    public int TotalEnrolledCourses { get; set; }

    /// <summary>
    /// Số course đã hoàn thành
    /// </summary>
    public int CompletedCourses { get; set; }

    /// <summary>
    /// Số course đang học
    /// </summary>
    public int InProgressCourses { get; set; }

    /// <summary>
    /// Số course chưa bắt đầu
    /// </summary>
    public int NotStartedCourses { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành trung bình (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }
}

/// <summary>
/// Chart data point cho Course Statistics
/// </summary>
public class StudentCourseStatisticsChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Tên course
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// ID course
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành (%)
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Số lesson items đã hoàn thành
    /// </summary>
    public int CompletedLessonItems { get; set; }

    /// <summary>
    /// Tổng số lesson items
    /// </summary>
    public int TotalLessonItems { get; set; }

    /// <summary>
    /// Trạng thái: Completed, InProgress, NotStarted
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

