namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Chart data point cho Income Analytics
/// </summary>
public class IncomeChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Tháng (1-12)
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Tổng thu nhập
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Thu nhập đã nhận
    /// </summary>
    public decimal ReceivedIncome { get; set; }
}

/// <summary>
/// Chart data point cho Course Analytics
/// </summary>
public class CourseChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số lượng course
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Trạng thái (Pending, Open, Rejected, Draft)
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Chart data point cho Student Learning Ability
/// </summary>
public class StudentLearningChartDataPoint : BaseChartDataPoint
{
    /// <summary>
    /// Số course đã đăng ký
    /// </summary>
    public int TotalEnrolledCourses { get; set; }

    /// <summary>
    /// Số course đã hoàn thành
    /// </summary>
    public int CompletedCourses { get; set; }

    /// <summary>
    /// Số course đang học
    /// </summary>
    public int InProgressCourses { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành trung bình (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }
}

