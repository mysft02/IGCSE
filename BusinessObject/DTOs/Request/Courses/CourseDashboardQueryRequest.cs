using BusinessObject.DTOs.Response.Courses;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseDashboardQueryRequest : BaseQueryRequest
    {
        public int? CourseId { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Course))
            {
                throw new NotSupportedException($"CourseDashboardQueryRequest only supports Course type, got {typeof(T).Name}");
            }

            return BuildCourseFilter() as Expression<Func<T, bool>>;
        }

        public List<CourseDashboardQueryResponse> ApplySorting(List<CourseDashboardQueryResponse> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "courseid" => isAscending
                    ? tests.OrderBy(t => t.CourseId).ToList()
                    : tests.OrderByDescending(t => t.CourseId).ToList(),
                "coursename" => isAscending
                    ? tests.OrderBy(t => t.CourseName).ToList()
                    : tests.OrderByDescending(t => t.CourseName).ToList(),
                "coursedescription" => isAscending
                    ? tests.OrderBy(t => t.CourseDescription).ToList()
                    : tests.OrderByDescending(t => t.CourseDescription).ToList(),
                "status" => isAscending
                    ? tests.OrderBy(t => t.Status).ToList()
                    : tests.OrderByDescending(t => t.Status).ToList(),
                "price" => isAscending
                    ? tests.OrderBy(t => t.Price).ToList()
                    : tests.OrderByDescending(t => t.Price).ToList(),
                "createdat" => isAscending
                    ? tests.OrderBy(t => t.CreatedAt).ToList()
                    : tests.OrderByDescending(t => t.CreatedAt).ToList(),
                "updatedat" => isAscending
                    ? tests.OrderBy(t => t.UpdatedAt).ToList()
                    : tests.OrderByDescending(t => t.UpdatedAt).ToList(),
                "createdby" => isAscending
                    ? tests.OrderBy(t => t.CreatedBy).ToList()
                    : tests.OrderByDescending(t => t.CreatedBy).ToList(),
                "customercount" => isAscending
                    ? tests.OrderBy(t => t.CustomerCount).ToList()
                    : tests.OrderByDescending(t => t.CustomerCount).ToList(),
                "averagefinalscore" => isAscending
                    ? tests.OrderBy(t => t.AverageFinalScore).ToList()
                    : tests.OrderByDescending(t => t.AverageFinalScore).ToList(),
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

            if (!string.IsNullOrEmpty(CourseId?.ToString()))
            {
                predicates.Add(x => x.CourseId == CourseId);
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
