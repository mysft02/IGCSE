namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Admin Enrollment Statistics API
/// </summary>
public class AdminEnrollmentStatisticsResponse : BaseAnalyticsResponse<AdminEnrollmentStatisticsMetadata, AdminEnrollmentStatisticsChartDataPoint>
{
}

/// <summary>
/// Metadata cho Enrollment Statistics
/// </summary>
public class AdminEnrollmentStatisticsMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số đăng ký
    /// </summary>
    public int TotalEnrollments { get; set; }

    /// <summary>
    /// Số học viên đang học (có process)
    /// </summary>
    public int ActiveStudents { get; set; }

    /// <summary>
    /// Số học viên đã hoàn thành ít nhất 1 course
    /// </summary>
    public int CompletedStudents { get; set; }
}

/// <summary>
/// Chart data point cho Enrollment Statistics
/// </summary>
public class AdminEnrollmentStatisticsChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số đăng ký mới trong khoảng thời gian này
    /// </summary>
    public int NewEnrollments { get; set; }

    /// <summary>
    /// Số học viên hoàn thành course trong khoảng thời gian này
    /// </summary>
    public int CompletedEnrollments { get; set; }
}

