namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Top Students API
/// </summary>
public class TopStudentsResponse
{
    /// <summary>
    /// Danh sách top học viên tiềm năng
    /// </summary>
    public List<TopStudentItem> Students { get; set; } = new List<TopStudentItem>();

    /// <summary>
    /// Tổng số học viên
    /// </summary>
    public int TotalStudents { get; set; }
}

/// <summary>
/// Thông tin một học viên trong top
/// </summary>
public class TopStudentItem
{
    /// <summary>
    /// ID học viên
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Tên học viên
    /// </summary>
    public string? StudentName { get; set; }

    /// <summary>
    /// Email học viên
    /// </summary>
    public string? StudentEmail { get; set; }

    /// <summary>
    /// Số course đã đăng ký
    /// </summary>
    public int TotalEnrolledCourses { get; set; }

    /// <summary>
    /// Số course đã hoàn thành
    /// </summary>
    public int CompletedCourses { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành trung bình (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }

    /// <summary>
    /// Tổng số lesson items đã hoàn thành
    /// </summary>
    public int TotalCompletedLessonItems { get; set; }

    /// <summary>
    /// Điểm trung bình các quiz/final quiz
    /// </summary>
    public decimal? AverageScore { get; set; }
}

