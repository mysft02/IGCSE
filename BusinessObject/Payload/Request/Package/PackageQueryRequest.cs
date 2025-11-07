using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.Payload.Request
{
    public class PackageQueryRequest : BaseQueryRequest
    {
        public int? PackageId { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Package))
            {
                throw new NotSupportedException($"PackageQueryRequest only supports Package type, got {typeof(T).Name}");
            }

            return BuildPackageFilter() as Expression<Func<T, bool>>;
        }

        public List<Package> ApplySorting(List<Package> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "packageid" => isAscending
                    ? tests.OrderBy(t => t.PackageId).ToList()
                    : tests.OrderByDescending(t => t.PackageId).ToList(),
                "title" => isAscending
                    ? tests.OrderBy(t => t.Title).ToList()
                    : tests.OrderByDescending(t => t.Title).ToList(),
                "description" => isAscending
                    ? tests.OrderBy(t => t.Description).ToList()
                    : tests.OrderByDescending(t => t.Description).ToList(),
                "price" => isAscending
                    ? tests.OrderBy(t => t.Price).ToList()
                    : tests.OrderByDescending(t => t.Price).ToList(),
                "slot" => isAscending
                    ? tests.OrderBy(t => t.Slot).ToList()
                    : tests.OrderByDescending(t => t.Slot).ToList(),
                "ismocktest" => isAscending
                    ? tests.OrderBy(t => t.IsMockTest).ToList()
                    : tests.OrderByDescending(t => t.IsMockTest).ToList(),
                "createdat" => isAscending
                    ? tests.OrderBy(t => t.CreatedAt).ToList()
                    : tests.OrderByDescending(t => t.CreatedAt).ToList(),
                "updatedat" => isAscending
                    ? tests.OrderBy(t => t.UpdatedAt).ToList()
                    : tests.OrderByDescending(t => t.UpdatedAt).ToList(),
                _ => tests
            };
        }

        public List<Package> ApplyPagination(List<Package> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Package, bool>>? BuildPackageFilter()
        {
            var predicates = new List<Expression<Func<Package, bool>>>();

            if (!string.IsNullOrEmpty(PackageId?.ToString()))
            {
                predicates.Add(x => x.PackageId == PackageId);
            }

            if(userID != null)
            {
                predicates.Add(x => x.Userpackages.Any(up => up.UserId == userID));
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Package), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Package, bool>>(combined, parameter);
            });
        }
    }   
}
