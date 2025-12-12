namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Top Analytics API (gộp Top Students và Top Courses)
/// </summary>
public class TopAnalyticsResponse
{
    /// <summary>
    /// Danh sách top học viên tiềm năng
    /// </summary>
    public List<TopStudentItem> TopStudents { get; set; } = new List<TopStudentItem>();

    /// <summary>
    /// Tổng số học viên
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Danh sách top khoá học được học nhiều nhất
    /// </summary>
    public List<TopCourseItem> TopCourses { get; set; } = new List<TopCourseItem>();

    /// <summary>
    /// Tổng số khoá học
    /// </summary>
    public int TotalCourses { get; set; }
}

