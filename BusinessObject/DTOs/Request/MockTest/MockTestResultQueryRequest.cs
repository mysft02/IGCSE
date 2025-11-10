using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.MockTest
{
    public class MockTestResultQueryRequest : BaseQueryRequest
    {
        public int? MockTestId { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Mocktestresult))
            {
                throw new NotSupportedException($"MockTestResultQueryRequest only supports Mocktestresult type, got {typeof(T).Name}");
            }

            return BuildMockTestResultFilter() as Expression<Func<T, bool>>;
        }

        public List<Mocktestresult> ApplySorting(List<Mocktestresult> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "mocktestresultid" => isAscending
                    ? tests.OrderBy(t => t.MockTestResultId).ToList()
                    : tests.OrderByDescending(t => t.MockTestResultId).ToList(),
                "mocktestid" => isAscending
                    ? tests.OrderBy(t => t.MockTestId).ToList()
                    : tests.OrderByDescending(t => t.MockTestId).ToList(),
                "score" => isAscending
                    ? tests.OrderBy(t => t.Score).ToList()
                    : tests.OrderByDescending(t => t.Score).ToList(),
                "userid" => isAscending
                    ? tests.OrderBy(t => t.UserId).ToList()
                    : tests.OrderByDescending(t => t.UserId).ToList(),
                "createdat" => isAscending
                    ? tests.OrderBy(t => t.CreatedAt).ToList()
                    : tests.OrderByDescending(t => t.CreatedAt).ToList(),
                _ => tests
            };
        }

        public List<Mocktestresult> ApplyPagination(List<Mocktestresult> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Mocktestresult, bool>>? BuildMockTestResultFilter()
        {
            var predicates = new List<Expression<Func<Mocktestresult, bool>>>();

            if (!string.IsNullOrEmpty(MockTestId?.ToString()))
            {
                predicates.Add(x => x.MockTestId == MockTestId);
            }

            if(userID != null)
            {
                predicates.Add(x => x.UserId == userID);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Mocktestresult), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Mocktestresult, bool>>(combined, parameter);
            });
        }
    }
}
