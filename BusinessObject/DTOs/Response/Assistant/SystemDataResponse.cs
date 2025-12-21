using BusinessObject.DTOs.Response.Dashboards;

namespace BusinessObject.DTOs.Response.Assistant;

/// <summary>
/// Structured system data response để frontend có thể render đẹp
/// </summary>
public class SystemDataResponse
{
    /// <summary>
    /// Loại dữ liệu: analytics, recommendations, statistics, progress, performance, certificates, overview
    /// </summary>
    public string DataType { get; set; } = null!;

    /// <summary>
    /// Title để hiển thị
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Structured data dựa trên DataType - sử dụng các DTOs từ Dashboards
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Metadata bổ sung
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

