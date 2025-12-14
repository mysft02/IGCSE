using Common.Utils;
using System.Linq.Expressions;
using BusinessObject.Model;

namespace BusinessObject.Payload.Request.Filter;

/// <summary>
/// Filter request cho Student Learning Ability Analytics
/// </summary>
public class StudentLearningAbilityQueryRequest : BaseQueryRequest
{
    /// <summary>
    /// Filter theo Course ID (optional)
    /// </summary>
    public int? CourseId { get; set; }

    /// <summary>
    /// Filter theo Student ID (optional)
    /// </summary>
    public string? StudentId { get; set; }

    /// <summary>
    /// Filter theo Teacher ID - chỉ lấy học viên của teacher này (required for teacher analytics)
    /// </summary>
    public string? TeacherId { get; set; }

    /// <summary>
    /// Filter theo ngày bắt đầu (optional)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter theo ngày kết thúc (optional)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Chỉ lấy học viên đã hoàn thành course (optional)
    /// </summary>
    public bool? OnlyCompletedCourses { get; set; }

    /// <summary>
    /// Minimum completion percentage để filter (0-100)
    /// </summary>
    public decimal? MinCompletionPercentage { get; set; }

    public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
    {
        // This method is generic but we only support Processitem for this query
        if (typeof(T) != typeof(Processitem))
        {
            throw new NotSupportedException($"StudentLearningAbilityQueryRequest only supports Processitem type, got {typeof(T).Name}");
        }

        return BuildProcessitemFilter() as Expression<Func<T, bool>>;
    }

    private Expression<Func<Processitem, bool>>? BuildProcessitemFilter()
    {
        var predicates = new List<Expression<Func<Processitem, bool>>>();

        // Filter by CourseId
        if (CourseId.HasValue)
        {
            predicates.Add(x => x.Process.CourseId == CourseId.Value);
        }

        // Filter by StudentId
        if (!CommonUtils.IsEmptyString(StudentId))
        {
            predicates.Add(x => x.Process.StudentId == StudentId);
        }

        // Filter by TeacherId (courses created by this teacher)
        if (!CommonUtils.IsEmptyString(TeacherId))
        {
            predicates.Add(x => x.Process.Course.CreatedBy == TeacherId);
        }

        // Filter by date range
        if (StartDate.HasValue)
        {
            var startDate = StartDate.Value;
            predicates.Add(x => x.CreatedAt >= startDate);
        }

        if (EndDate.HasValue)
        {
            var endDate = EndDate.Value;
            predicates.Add(x => x.CreatedAt <= endDate);
        }

        // Combine all predicates with AND
        if (!predicates.Any())
            return null;

        return predicates.Aggregate((expr1, expr2) =>
        {
            var parameter = Expression.Parameter(typeof(Processitem), "x");
            var body1 = Expression.Invoke(expr1, parameter);
            var body2 = Expression.Invoke(expr2, parameter);
            var combined = Expression.AndAlso(body1, body2);
            return Expression.Lambda<Func<Processitem, bool>>(combined, parameter);
        });
    }
}

