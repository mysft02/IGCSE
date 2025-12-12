namespace BusinessObject.DTOs.Response.Dashboards;

/// <summary>
/// Response cho Course Recommendation API
/// </summary>
public class CourseRecommendationResponse
{
    /// <summary>
    /// Danh sách courses được gợi ý
    /// </summary>
    public List<RecommendedCourseItem> RecommendedCourses { get; set; } = new List<RecommendedCourseItem>();

    /// <summary>
    /// Tổng số courses được gợi ý
    /// </summary>
    public int TotalRecommended { get; set; }

    /// <summary>
    /// Thông tin thành tích học tập của học viên (để hiển thị)
    /// </summary>
    public StudentAchievementInfo StudentAchievement { get; set; } = new StudentAchievementInfo();
}

/// <summary>
/// Thông tin một course được gợi ý
/// </summary>
public class RecommendedCourseItem
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
    /// Điểm tương đồng (0-1)
    /// </summary>
    public decimal SimilarityScore { get; set; }

    /// <summary>
    /// Điểm tổng hợp (dựa trên nhiều yếu tố)
    /// </summary>
    public decimal RecommendationScore { get; set; }

    /// <summary>
    /// Lý do được gợi ý
    /// </summary>
    public string RecommendationReason { get; set; } = string.Empty;
}

/// <summary>
/// Thông tin thành tích học tập của học viên
/// </summary>
public class StudentAchievementInfo
{
    /// <summary>
    /// Số course đã hoàn thành
    /// </summary>
    public int CompletedCourses { get; set; }

    /// <summary>
    /// Số chứng chỉ đã có
    /// </summary>
    public int TotalCertificates { get; set; }

    /// <summary>
    /// Tổng số lesson items đã hoàn thành
    /// </summary>
    public int TotalCompletedLessonItems { get; set; }

    /// <summary>
    /// Điểm trung bình các quiz/final quiz
    /// </summary>
    public decimal? AverageScore { get; set; }
}

