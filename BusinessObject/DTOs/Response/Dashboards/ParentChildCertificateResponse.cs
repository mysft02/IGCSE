namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Parent Child Certificate API
/// </summary>
public class ParentChildCertificateResponse
{
    /// <summary>
    /// Tổng số chứng chỉ của tất cả con
    /// </summary>
    public int TotalCertificates { get; set; }

    /// <summary>
    /// Danh sách chứng chỉ của các con
    /// </summary>
    public List<ChildCertificateItem> Certificates { get; set; } = new List<ChildCertificateItem>();

    /// <summary>
    /// Thống kê theo từng con
    /// </summary>
    public List<ChildCertificateStatistic> ChildrenStatistics { get; set; } = new List<ChildCertificateStatistic>();
}

/// <summary>
/// Thông tin một chứng chỉ
/// </summary>
public class ChildCertificateItem
{
    /// <summary>
    /// ID chứng chỉ
    /// </summary>
    public int CertificateId { get; set; }

    /// <summary>
    /// ID khoá học
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Tên khoá học
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// ID con
    /// </summary>
    public string ChildId { get; set; } = string.Empty;

    /// <summary>
    /// Tên con
    /// </summary>
    public string ChildName { get; set; } = string.Empty;

    /// <summary>
    /// URL hình ảnh chứng chỉ
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Ngày nhận chứng chỉ
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Thống kê chứng chỉ theo từng con
/// </summary>
public class ChildCertificateStatistic
{
    /// <summary>
    /// ID con
    /// </summary>
    public string ChildId { get; set; } = string.Empty;

    /// <summary>
    /// Tên con
    /// </summary>
    public string ChildName { get; set; } = string.Empty;

    /// <summary>
    /// Số chứng chỉ đã có
    /// </summary>
    public int CertificateCount { get; set; }

    /// <summary>
    /// Danh sách chứng chỉ
    /// </summary>
    public List<ChildCertificateItem> Certificates { get; set; } = new List<ChildCertificateItem>();
}

