namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Admin Overview API (tổng quan)
/// </summary>
public class AdminOverviewResponse
{
    /// <summary>
    /// Tổng quan tài khoản
    /// </summary>
    public AccountOverview Accounts { get; set; } = new AccountOverview();

    /// <summary>
    /// Tổng quan courses
    /// </summary>
    public CourseOverview Courses { get; set; } = new CourseOverview();

    /// <summary>
    /// Tổng quan doanh thu
    /// </summary>
    public RevenueOverview Revenue { get; set; } = new RevenueOverview();

    /// <summary>
    /// Tổng quan đăng ký
    /// </summary>
    public EnrollmentOverview Enrollments { get; set; } = new EnrollmentOverview();
}

/// <summary>
/// Tổng quan tài khoản
/// </summary>
public class AccountOverview
{
    /// <summary>
    /// Tổng số tài khoản
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Số giáo viên
    /// </summary>
    public int Teachers { get; set; }

    /// <summary>
    /// Số học viên
    /// </summary>
    public int Students { get; set; }

    /// <summary>
    /// Số phụ huynh
    /// </summary>
    public int Parents { get; set; }

    /// <summary>
    /// Số tài khoản mới trong tháng này
    /// </summary>
    public int NewThisMonth { get; set; }
}

/// <summary>
/// Tổng quan courses
/// </summary>
public class CourseOverview
{
    /// <summary>
    /// Tổng số courses
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Số courses đang Open
    /// </summary>
    public int Open { get; set; }

    /// <summary>
    /// Số courses đang Pending
    /// </summary>
    public int Pending { get; set; }

    /// <summary>
    /// Số courses bị Rejected
    /// </summary>
    public int Rejected { get; set; }

    /// <summary>
    /// Số courses mới trong tháng này
    /// </summary>
    public int NewThisMonth { get; set; }
}

/// <summary>
/// Tổng quan doanh thu
/// </summary>
public class RevenueOverview
{
    /// <summary>
    /// Tổng doanh thu
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Doanh thu tháng này
    /// </summary>
    public decimal ThisMonth { get; set; }

    /// <summary>
    /// Doanh thu tháng trước
    /// </summary>
    public decimal LastMonth { get; set; }

    /// <summary>
    /// Tổng số giao dịch
    /// </summary>
    public int TotalTransactions { get; set; }
}

/// <summary>
/// Tổng quan đăng ký
/// </summary>
public class EnrollmentOverview
{
    /// <summary>
    /// Tổng số đăng ký
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Số đăng ký tháng này
    /// </summary>
    public int ThisMonth { get; set; }

    /// <summary>
    /// Số học viên đang học
    /// </summary>
    public int ActiveStudents { get; set; }

    /// <summary>
    /// Số học viên đã hoàn thành
    /// </summary>
    public int CompletedStudents { get; set; }
}

