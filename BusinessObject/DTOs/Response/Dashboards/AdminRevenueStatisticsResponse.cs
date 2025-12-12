namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Admin Revenue Statistics API
/// </summary>
public class AdminRevenueStatisticsResponse : BaseAnalyticsResponse<AdminRevenueStatisticsMetadata, AdminRevenueStatisticsChartDataPoint>
{
}

/// <summary>
/// Metadata cho Revenue Statistics
/// </summary>
public class AdminRevenueStatisticsMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng doanh thu
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Tổng số giao dịch
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Doanh thu trung bình mỗi giao dịch
    /// </summary>
    public decimal AverageTransactionAmount { get; set; }
}

/// <summary>
/// Chart data point cho Revenue Statistics
/// </summary>
public class AdminRevenueStatisticsChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Doanh thu trong khoảng thời gian này
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Số giao dịch trong khoảng thời gian này
    /// </summary>
    public int TransactionCount { get; set; }
}

