using BusinessObject.DTOs.Response.Packages;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Packages
{
    public class PackageQueryRequest : BaseQueryRequest
    {
        public string? Role { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Package))
            {
                throw new NotSupportedException($"PackageQueryRequest only supports Package type, got {typeof(T).Name}");
            }

            return BuildPackageFilter() as Expression<Func<T, bool>>;
        }

        public List<PackageQueryResponse> ApplySorting(List<PackageQueryResponse> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "packageid" => isAscending
                    ? tests.OrderBy(t => t.PackageId).ToList()
                    : tests.OrderByDescending(t => t.PackageId).ToList(),
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

            if (!string.IsNullOrEmpty(Role))
            {
                if (Role.Contains("Parent") || Role.Contains("Student"))
                {
                    predicates.Add(x => x.IsMockTest == true);
                }
                else if (Role.Contains("Teacher"))
                {
                    predicates.Add(x => x.IsMockTest == false);
                }
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
