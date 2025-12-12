namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Top Courses API
/// </summary>
public class TopCoursesResponse
{
    /// <summary>
    /// Danh sách top khoá học được học nhiều nhất
    /// </summary>
    public List<TopCourseItem> Courses { get; set; } = new List<TopCourseItem>();

    /// <summary>
    /// Tổng số khoá học
    /// </summary>
    public int TotalCourses { get; set; }
}

/// <summary>
/// Thông tin một khoá học trong top
/// </summary>
public class TopCourseItem
{
    /// <summary>
    /// ID khoá học
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Tên khoá học
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả khoá học
    /// </summary>
    public string? CourseDescription { get; set; }

    /// <summary>
    /// URL hình ảnh
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Giá khoá học
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Số học viên đã đăng ký
    /// </summary>
    public int TotalEnrollments { get; set; }

    /// <summary>
    /// Số học viên đang học (có process)
    /// </summary>
    public int ActiveStudents { get; set; }

    /// <summary>
    /// Số học viên đã hoàn thành
    /// </summary>
    public int CompletedStudents { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành trung bình (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }

    /// <summary>
    /// Điểm trung bình final quiz
    /// </summary>
    public decimal? AverageFinalScore { get; set; }

    /// <summary>
    /// Tổng thu nhập từ khoá học
    /// </summary>
    public decimal TotalIncome { get; set; }
}

