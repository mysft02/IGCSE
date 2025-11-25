using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Packages
{
    public class PackageOwnedQueryRequest : BaseQueryRequest
    {
        public int? PackageId { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Userpackage))
            {
                throw new NotSupportedException($"PackageOwnedQueryRequest only supports Package type, got {typeof(T).Name}");
            }

            return BuildPackageFilter() as Expression<Func<T, bool>>;
        }

        public List<Userpackage> ApplySorting(List<Userpackage> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "userid" => isAscending
                    ? tests.OrderBy(t => t.UserId).ToList()
                    : tests.OrderByDescending(t => t.UserId).ToList(),
                "price" => isAscending
                    ? tests.OrderBy(t => t.Price).ToList()
                    : tests.OrderByDescending(t => t.Price).ToList(),
                "isactive" => isAscending
                    ? tests.OrderBy(t => t.IsActive).ToList()
                    : tests.OrderByDescending(t => t.IsActive).ToList(),
                "createdat" => isAscending
                    ? tests.OrderBy(t => t.CreatedAt).ToList()
                    : tests.OrderByDescending(t => t.CreatedAt).ToList(),
                "updatedat" => isAscending
                    ? tests.OrderBy(t => t.UpdatedAt).ToList()
                    : tests.OrderByDescending(t => t.UpdatedAt).ToList(),
                _ => tests
            };
        }

        public List<Userpackage> ApplyPagination(List<Userpackage> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Userpackage, bool>>? BuildPackageFilter()
        {
            var predicates = new List<Expression<Func<Userpackage, bool>>>();

            if (!string.IsNullOrEmpty(userID))
            {
                predicates.Add(x => x.UserId == userID);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Userpackage), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Userpackage, bool>>(combined, parameter);
            });
        }
    }
}
