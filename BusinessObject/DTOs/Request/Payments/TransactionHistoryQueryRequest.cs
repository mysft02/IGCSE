using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Payments
{
    public class TransactionHistoryQueryRequest : BaseQueryRequest
    {
        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Transactionhistory))
            {
                throw new NotSupportedException($"TransactionHistoryQueryRequest only supports Package type, got {typeof(T).Name}");
            }

            return BuildTransactionHistoryFilter() as Expression<Func<T, bool>>;
        }

        public List<Transactionhistory> ApplySorting(List<Transactionhistory> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "userid" => isAscending
                    ? tests.OrderBy(t => t.UserId).ToList()
                    : tests.OrderByDescending(t => t.UserId).ToList(),
                _ => tests
            };
        }

        public List<Transactionhistory> ApplyPagination(List<Transactionhistory> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Transactionhistory, bool>>? BuildTransactionHistoryFilter()
        {
            var predicates = new List<Expression<Func<Transactionhistory, bool>>>();

            if (!string.IsNullOrEmpty(userID))
            {
                predicates.Add(x => x.UserId == userID);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Transactionhistory), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Transactionhistory, bool>>(combined, parameter);
            });
        }
    }
}
