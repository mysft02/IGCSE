namespace BusinessObject.Model;

public class Coursereport
{
    public int CourseReportId { get; set; }

    public int CourseId { get; set; }

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    /// <summary>
    /// Trạng thái xử lý report: PROCESSING, COMPLETED
    /// </summary>
    public string Status { get; set; } = "PROCESSING";

    /// <summary>
    /// Loại report: nội dung không phù hợp, trùng lặp, ...
    /// </summary>
    public string? ReportType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

