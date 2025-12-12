namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Parent Child Performance API
/// </summary>
public class ParentChildPerformanceResponse : BaseAnalyticsResponse<ParentChildPerformanceMetadata, ParentChildPerformanceChartDataPoint>
{
}

/// <summary>
/// Metadata cho Child Performance
/// </summary>
public class ParentChildPerformanceMetadata
{
    /// <summary>
    /// Tổng số con
    /// </summary>
    public int TotalChildren { get; set; }

    /// <summary>
    /// Điểm trung bình của tất cả con
    /// </summary>
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Số quiz đã làm
    /// </summary>
    public int TotalQuizzesTaken { get; set; }

    /// <summary>
    /// Số quiz đã pass
    /// </summary>
    public int PassedQuizzes { get; set; }

    /// <summary>
    /// Tỷ lệ pass quiz (%)
    /// </summary>
    public decimal QuizPassRate { get; set; }
}

/// <summary>
/// Chart data point cho Child Performance
/// </summary>
public class ParentChildPerformanceChartDataPoint : BaseChartDataPoint
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
    /// Điểm trung bình
    /// </summary>
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Số quiz đã làm
    /// </summary>
    public int QuizzesTaken { get; set; }

    /// <summary>
    /// Số quiz đã pass
    /// </summary>
    public int PassedQuizzes { get; set; }

    /// <summary>
    /// Tỷ lệ pass (%)
    /// </summary>
    public decimal PassRate { get; set; }
}

