namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Parent Child Learning Progress API
/// </summary>
public class ParentChildLearningProgressResponse : BaseAnalyticsResponse<ParentChildLearningProgressMetadata, ParentChildLearningProgressChartDataPoint>
{
}

/// <summary>
/// Metadata cho Child Learning Progress
/// </summary>
public class ParentChildLearningProgressMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số con
    /// </summary>
    public int TotalChildren { get; set; }

    /// <summary>
    /// Tổng số lesson items đã hoàn thành bởi tất cả con
    /// </summary>
    public int TotalCompletedLessonItems { get; set; }

    /// <summary>
    /// Tổng số giờ học của tất cả con
    /// </summary>
    public decimal TotalStudyHours { get; set; }
}

/// <summary>
/// Chart data point cho Child Learning Progress
/// </summary>
public class ParentChildLearningProgressChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số lesson items đã hoàn thành trong khoảng thời gian này
    /// </summary>
    public int CompletedLessonItems { get; set; }

    /// <summary>
    /// Số giờ học trong khoảng thời gian này
    /// </summary>
    public decimal StudyHours { get; set; }

    /// <summary>
    /// Số con đang học trong khoảng thời gian này
    /// </summary>
    public int ActiveChildren { get; set; }
}

