using System.Linq.Expressions;

namespace BusinessObject.Payload.Request.Filter;

public abstract class BaseQueryRequest
{
    public string? Query { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;
    public string? userID { get; set; }

    public int GetPageSize()
    {
        return Size > 0 ? Size : 10;
    }

    public abstract Expression<Func<T, bool>>? BuildFilter<T>() where T : class;

    public IQueryable<T> ApplySorting<T>(IQueryable<T> query) where T : class
    {
        if (string.IsNullOrEmpty(SortBy))
            return query;

        if (string.IsNullOrEmpty(SortOrder) || 
            (!SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) && 
             !SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)))
        {
            return query;
        }

        // Note: For dynamic sorting, you might need to use reflection or a more sophisticated approach
        // This is a simplified version
        return query;
    }

    public IQueryable<T> ApplyPagination<T>(IQueryable<T> query) where T : class
    {
        return query.Skip(Page * GetPageSize()).Take(GetPageSize());
    }
}