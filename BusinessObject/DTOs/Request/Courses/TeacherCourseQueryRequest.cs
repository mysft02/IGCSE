using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Courses
{
    public class TeacherCourseQueryRequest : BaseQueryRequest
    {
        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Course))
            {
                throw new NotSupportedException($"TeacherCourseQueryRequest only supports Course type, got {typeof(T).Name}");
            }

            return BuildCourseFilter() as Expression<Func<T, bool>>;
        }

        public List<Course> ApplySorting(List<Course> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "enrollmentid" => isAscending
                    ? tests.OrderBy(t => t.CourseId).ToList()
                    : tests.OrderByDescending(t => t.CourseId).ToList(),
                _ => tests
            };
        }

        public List<Course> ApplyPagination(List<Course> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Course, bool>>? BuildCourseFilter()
        {
            var predicates = new List<Expression<Func<Course, bool>>>();

            if (!string.IsNullOrEmpty(userID))
            {
                predicates.Add(x => x.CreatedBy == userID);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Course), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Course, bool>>(combined, parameter);
            });
        }
    }
}


