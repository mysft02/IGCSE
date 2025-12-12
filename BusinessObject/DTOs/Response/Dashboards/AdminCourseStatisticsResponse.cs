namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Admin Course Statistics API
/// </summary>
public class AdminCourseStatisticsResponse : BaseAnalyticsResponse<AdminCourseStatisticsMetadata, AdminCourseStatisticsChartDataPoint>
{
}

/// <summary>
/// Metadata cho Course Statistics
/// </summary>
public class AdminCourseStatisticsMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số courses
    /// </summary>
    public int TotalCourses { get; set; }

    /// <summary>
    /// Số courses mới tạo
    /// </summary>
    public int NewCourses { get; set; }

    /// <summary>
    /// Số courses đang Open
    /// </summary>
    public int OpenCourses { get; set; }

    /// <summary>
    /// Số courses đang Pending
    /// </summary>
    public int PendingCourses { get; set; }
}

/// <summary>
/// Chart data point cho Course Statistics
/// </summary>
public class AdminCourseStatisticsChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số courses mới tạo trong khoảng thời gian này
    /// </summary>
    public int NewCourses { get; set; }

    /// <summary>
    /// Số courses được approve (chuyển sang Open)
    /// </summary>
    public int ApprovedCourses { get; set; }
}

