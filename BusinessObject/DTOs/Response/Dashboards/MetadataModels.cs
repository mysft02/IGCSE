namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Metadata cho Teacher Analytics
/// </summary>
public class TeacherAnalyticsMetadata
{
    /// <summary>
    /// Tổng số course
    /// </summary>
    public int TotalCourses { get; set; }

    /// <summary>
    /// Số course đang pending
    /// </summary>
    public int Pending { get; set; }

    /// <summary>
    /// Số course đang open
    /// </summary>
    public int Open { get; set; }

    /// <summary>
    /// Số course bị rejected
    /// </summary>
    public int Rejected { get; set; }

    /// <summary>
    /// Số course ở draft
    /// </summary>
    public int Draft { get; set; }
}

/// <summary>
/// Metadata cho Lesson Completion Analytics
/// </summary>
public class LessonCompletionMetadata
{
    /// <summary>
    /// Đơn vị thời gian được sử dụng (Hour, Day, Month)
    /// </summary>
    public string TimeUnit { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số người đã hoàn thành lesson trong khoảng thời gian
    /// </summary>
    public int TotalCompletedUsers { get; set; }

    /// <summary>
    /// Tổng số lesson items đã được hoàn thành trong khoảng thời gian
    /// </summary>
    public int TotalCompletedLessonItems { get; set; }
}

/// <summary>
/// Metadata cho Course Count
/// </summary>
public class CourseCountMetadata
{
    /// <summary>
    /// Tổng số course
    /// </summary>
    public int TotalCourses { get; set; }

    /// <summary>
    /// Số course đang pending
    /// </summary>
    public int Pending { get; set; }

    /// <summary>
    /// Số course đang open
    /// </summary>
    public int Open { get; set; }

    /// <summary>
    /// Số course bị rejected
    /// </summary>
    public int Rejected { get; set; }

    /// <summary>
    /// Số course ở draft
    /// </summary>
    public int Draft { get; set; }
}

/// <summary>
/// Metadata cho Admin Analytics
/// </summary>
public class AdminAnalyticsMetadata
{
    /// <summary>
    /// Thống kê course
    /// </summary>
    public CourseCountMetadata Course { get; set; } = new CourseCountMetadata();

    /// <summary>
    /// Thống kê tài khoản
    /// </summary>
    public AccountCountMetadata Accounts { get; set; } = new AccountCountMetadata();
}

/// <summary>
/// Metadata cho Account Count
/// </summary>
public class AccountCountMetadata
{
    /// <summary>
    /// Tổng số tài khoản
    /// </summary>
    public int TotalAccounts { get; set; }

    /// <summary>
    /// Số giáo viên
    /// </summary>
    public int Teacher { get; set; }

    /// <summary>
    /// Số phụ huynh
    /// </summary>
    public int Parent { get; set; }

    /// <summary>
    /// Số học viên
    /// </summary>
    public int Student { get; set; }
}

/// <summary>
/// Metadata cho Student Learning Ability
/// </summary>
public class StudentLearningAbilityMetadata
{
    /// <summary>
    /// Tổng số học viên
    /// </summary>
    public int TotalStudents { get; set; }

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

