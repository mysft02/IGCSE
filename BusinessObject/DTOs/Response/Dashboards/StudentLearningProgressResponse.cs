namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Learning Progress Chart API
/// </summary>
public class StudentLearningProgressResponse : BaseAnalyticsResponse<StudentLearningProgressMetadata, StudentLearningProgressChartDataPoint>
{
}

/// <summary>
/// Metadata cho Learning Progress
/// </summary>
public class StudentLearningProgressMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số lesson items đã hoàn thành
    /// </summary>
    public int TotalCompletedLessonItems { get; set; }

    /// <summary>
    /// Tổng số giờ học
    /// </summary>
    public decimal TotalStudyHours { get; set; }

    /// <summary>
    /// Số course đang học
    /// </summary>
    public int ActiveCourses { get; set; }
}

/// <summary>
/// Chart data point cho Learning Progress
/// </summary>
public class StudentLearningProgressChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số lesson items đã hoàn thành trong khoảng thời gian này
    /// </summary>
    public int CompletedLessonItems { get; set; }

    /// <summary>
    /// Số giờ học trong khoảng thời gian này
    /// </summary>
    public decimal StudyHours { get; set; }
}

