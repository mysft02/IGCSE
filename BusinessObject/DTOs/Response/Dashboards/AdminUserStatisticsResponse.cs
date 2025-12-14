namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Admin User Statistics API
/// </summary>
public class AdminUserStatisticsResponse : BaseAnalyticsResponse<AdminUserStatisticsMetadata, AdminUserStatisticsChartDataPoint>
{
}

/// <summary>
/// Metadata cho User Statistics
/// </summary>
public class AdminUserStatisticsMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số tài khoản
    /// </summary>
    public int TotalAccounts { get; set; }

    /// <summary>
    /// Số giáo viên
    /// </summary>
    public int TotalTeachers { get; set; }

    /// <summary>
    /// Số học viên
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Số phụ huynh
    /// </summary>
    public int TotalParents { get; set; }
}

/// <summary>
/// Chart data point cho User Statistics
/// </summary>
public class AdminUserStatisticsChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số tài khoản mới đăng ký trong khoảng thời gian này
    /// </summary>
    public int NewAccounts { get; set; }

    /// <summary>
    /// Số giáo viên mới
    /// </summary>
    public int NewTeachers { get; set; }

    /// <summary>
    /// Số học viên mới
    /// </summary>
    public int NewStudents { get; set; }
}

