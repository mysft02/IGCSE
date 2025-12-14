namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Student Learning Ability Analytics
/// </summary>
public class StudentLearningAbilityResponse
{
    /// <summary>
    /// Tổng số học viên
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Danh sách thống kê theo từng học viên
    /// </summary>
    public List<StudentLearningStatistic> Students { get; set; } = new List<StudentLearningStatistic>();

    /// <summary>
    /// Thống kê tổng quan
    /// </summary>
    public OverallLearningStatistic Overall { get; set; } = new OverallLearningStatistic();
}

/// <summary>
/// Thống kê học tập của một học viên
/// </summary>
public class StudentLearningStatistic
{
    /// <summary>
    /// Student ID
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
    /// Tổng số course đã đăng ký
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

    /// <summary>
    /// Chi tiết theo từng course
    /// </summary>
    public List<CourseCompletionDetail> CourseDetails { get; set; } = new List<CourseCompletionDetail>();
}

/// <summary>
/// Chi tiết hoàn thành course của học viên
/// </summary>
public class CourseCompletionDetail
{
    /// <summary>
    /// Course ID
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Tên course
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số lesson items trong course
    /// </summary>
    public int TotalLessonItems { get; set; }

    /// <summary>
    /// Số lesson items đã hoàn thành
    /// </summary>
    public int CompletedLessonItems { get; set; }

    /// <summary>
    /// Tỷ lệ hoàn thành (%)
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Đã hoàn thành course chưa
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Ngày bắt đầu học
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Ngày hoàn thành (nếu đã hoàn thành)
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Thống kê tổng quan
/// </summary>
public class OverallLearningStatistic
{
    /// <summary>
    /// Tỷ lệ hoàn thành trung bình của tất cả học viên (%)
    /// </summary>
    public decimal AverageCompletionPercentage { get; set; }

    /// <summary>
    /// Tổng số course đã hoàn thành bởi tất cả học viên
    /// </summary>
    public int TotalCompletedCourses { get; set; }

    /// <summary>
    /// Tổng số course đang học
    /// </summary>
    public int TotalInProgressCourses { get; set; }

    /// <summary>
    /// Số học viên đã hoàn thành ít nhất 1 course
    /// </summary>
    public int StudentsWithCompletedCourses { get; set; }

    /// <summary>
    /// Số học viên chưa hoàn thành course nào
    /// </summary>
    public int StudentsWithNoCompletedCourses { get; set; }
}

