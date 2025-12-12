namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Base response cho tất cả Analytics APIs
/// </summary>
/// <typeparam name="TMetadata">Type của metadata (chứa các thông tin tổng quan)</typeparam>
/// <typeparam name="TChartData">Type của chart data point</typeparam>
public class BaseAnalyticsResponse<TMetadata, TChartData>
    where TMetadata : class
    where TChartData : BaseChartDataPoint
{
    /// <summary>
    /// Metadata chứa các thông tin tổng quan (totalCourses, pending, open, etc.)
    /// </summary>
    public TMetadata Metadata { get; set; } = null!;

    /// <summary>
    /// Chart data chứa mảng các data points với label và các chỉ số
    /// </summary>
    public List<TChartData> Chart { get; set; } = new List<TChartData>();
}

/// <summary>
/// Base class cho chart data point
/// </summary>
public abstract class BaseChartDataPoint
{
    /// <summary>
    /// Label hiển thị cho data point (ví dụ: "2024-01", "January", "Week 1")
    /// </summary>
    public string Label { get; set; } = string.Empty;
}

