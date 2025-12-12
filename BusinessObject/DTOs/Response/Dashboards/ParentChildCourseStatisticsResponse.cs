namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Parent Child Course Statistics API
/// </summary>
public class ParentChildCourseStatisticsResponse : BaseAnalyticsResponse<ParentChildCourseStatisticsMetadata, ParentChildCourseStatisticsChartDataPoint>
{
}

/// <summary>
/// Metadata cho Child Course Statistics
/// </summary>
public class ParentChildCourseStatisticsMetadata
{
    /// <summary>
    /// Tổng số con
    /// </summary>
    public int TotalChildren { get; set; }

    /// <summary>
    /// Tổng số courses đã đăng ký của tất cả con
    /// </summary>
    public int TotalEnrolledCourses { get; set; }

    /// <summary>
    /// Số courses đã hoàn thành
    /// </summary>
    public int CompletedCourses { get; set; }

    /// <summary>
    /// Số courses đang học
    /// </summary>
    public int InProgressCourses { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành trung bình (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }
}

/// <summary>
/// Chart data point cho Child Course Statistics
/// </summary>
public class ParentChildCourseStatisticsChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Tên con
    /// </summary>
    public string ChildName { get; set; } = string.Empty;

    /// <summary>
    /// ID con
    /// </summary>
    public string ChildId { get; set; } = string.Empty;

    /// <summary>
    /// Số courses đã đăng ký
    /// </summary>
    public int EnrolledCourses { get; set; }

    /// <summary>
    /// Số courses đã hoàn thành
    /// </summary>
    public int CompletedCourses { get; set; }

    /// <summary>
    /// Số courses đang học
    /// </summary>
    public int InProgressCourses { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành trung bình (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }
}

