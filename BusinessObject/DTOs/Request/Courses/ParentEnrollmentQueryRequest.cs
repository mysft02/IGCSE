using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Courses
{
    public class ParentEnrollmentQueryRequest : BaseQueryRequest
    {
        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Studentenrollment))
            {
                throw new NotSupportedException($"ParentEnrollmentQueryRequest only supports Studentenrollment type, got {typeof(T).Name}");
            }

            return BuildStudentEnrollmentFilter() as Expression<Func<T, bool>>;
        }

        public List<Studentenrollment> ApplySorting(List<Studentenrollment> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "enrollmentid" => isAscending
                    ? tests.OrderBy(t => t.EnrollmentId).ToList()
                    : tests.OrderByDescending(t => t.EnrollmentId).ToList(),
                _ => tests
            };
        }

        public List<Studentenrollment> ApplyPagination(List<Studentenrollment> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Studentenrollment, bool>>? BuildStudentEnrollmentFilter()
        {
            var predicates = new List<Expression<Func<Studentenrollment, bool>>>();

            if (!string.IsNullOrEmpty(userID))
            {
                predicates.Add(x => x.ParentId == userID);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Studentenrollment), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Studentenrollment, bool>>(combined, parameter);
            });
        }
    }
}


