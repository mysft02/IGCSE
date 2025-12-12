namespace BusinessObject.Payload.Request.Filter;

/// <summary>
/// Filter request đơn giản cho Analytics APIs - chỉ cần date range
/// </summary>
public class AnalyticsDateRangeRequest
{
    /// <summary>
    /// Ngày bắt đầu (optional - mặc định: đầu năm hiện tại)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Ngày kết thúc (optional - mặc định: cuối năm hiện tại)
    /// </summary>
    public DateTime? EndDate { get; set; }
}

