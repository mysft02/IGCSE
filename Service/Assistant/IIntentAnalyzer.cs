namespace Service.Assistant;

/// <summary>
/// Intent được phân tích từ câu hỏi của user
/// </summary>
public class QueryIntent
{
    /// <summary>
    /// Loại dữ liệu cần lấy (analytics, top_courses, top_students, progress, statistics, recommendations, etc.)
    /// </summary>
    public List<string> DataTypes { get; set; } = new();

    /// <summary>
    /// Mức độ liên quan (0.0 - 1.0)
    /// </summary>
    public decimal RelevanceScore { get; set; }

    /// <summary>
    /// Có cần lấy dữ liệu hệ thống không
    /// </summary>
    public bool NeedsSystemData { get; set; }
}

public interface IIntentAnalyzer
{
    Task<QueryIntent> AnalyzeIntentAsync(string userQuery, string userRole);
}

