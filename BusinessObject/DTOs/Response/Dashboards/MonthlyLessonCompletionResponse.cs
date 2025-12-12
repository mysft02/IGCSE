using Common.Constants;

namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Lesson Completion Analytics (sử dụng pattern chung)
/// </summary>
public class LessonCompletionResponse : BaseAnalyticsResponse<LessonCompletionMetadata, LessonCompletionChartDataPoint>
{
}

/// <summary>
/// Chart data point cho Lesson Completion
/// </summary>
public class LessonCompletionChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số lượng người đã hoàn thành lesson
    /// </summary>
    public int CompletedUsersCount { get; set; }

    /// <summary>
    /// Tổng số lesson items đã được hoàn thành
    /// </summary>
    public int CompletedLessonItemsCount { get; set; }
}

