using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.Payload.Request.MockTest
{
    public class MockTestQueryRequest : BaseQueryRequest
    {
        public int MockTestId { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Mocktest))
            {
                throw new NotSupportedException($"MockTestQueryRequest only supports Mocktest type, got {typeof(T).Name}");
            }

            return BuildMockTestFilter() as Expression<Func<T, bool>>;
        }

        public List<Mocktest> ApplySorting(List<Mocktest> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "mocktestid" => isAscending
                    ? tests.OrderBy(t => t.MockTestId).ToList()
                    : tests.OrderByDescending(t => t.MockTestId).ToList(),
                "mocktesttitle" => isAscending
                    ? tests.OrderBy(t => t.MockTestTitle).ToList()
                    : tests.OrderByDescending(t => t.MockTestTitle).ToList(),
                "mocktestdescription" => isAscending
                    ? tests.OrderBy(t => t.MockTestDescription).ToList()
                    : tests.OrderByDescending(t => t.MockTestDescription).ToList(),
                "createdat" => isAscending
                    ? tests.OrderBy(t => t.CreatedAt).ToList()
                    : tests.OrderByDescending(t => t.CreatedAt).ToList(),
                "updatedat" => isAscending
                    ? tests.OrderBy(t => t.UpdatedAt).ToList()
                    : tests.OrderByDescending(t => t.UpdatedAt).ToList(),
                "createdby" => isAscending
                    ? tests.OrderBy(t => t.CreatedBy).ToList()
                    : tests.OrderByDescending(t => t.CreatedBy).ToList(),
                _ => tests
            };
        }

        public List<Mocktest> ApplyPagination(List<Mocktest> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Mocktest, bool>>? BuildMockTestFilter()
        {
            var predicates = new List<Expression<Func<Mocktest, bool>>>();

            if (MockTestId != null)
            {
                predicates.Add(x => x.MockTestId == MockTestId);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Mocktest), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Mocktest, bool>>(combined, parameter);
            });
        }
    }
}
