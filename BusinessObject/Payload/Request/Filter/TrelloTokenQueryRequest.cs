using Common.Utils;
using System.Linq.Expressions;
using BusinessObject.Model;

namespace BusinessObject.Payload.Request.Filter;

public class TrelloTokenQueryRequest : BaseQueryRequest
{
    public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
    {
        // This method is generic but we only support TrelloToken
        if (typeof(T) != typeof(TrelloToken))
        {
            throw new NotSupportedException($"TrelloTokenQueryRequest only supports TrelloToken type, got {typeof(T).Name}");
        }

        return BuildTrelloTokenFilter() as Expression<Func<T, bool>>;
    }

    /// <summary>
    /// Apply sorting to TrelloToken list
    /// </summary>
    public List<TrelloToken> ApplySorting(List<TrelloToken> tokens)
    {
        if (string.IsNullOrEmpty(SortBy))
            return tokens;

        var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

        return SortBy.ToLower() switch
        {
            "name" => isAscending 
                ? tokens.OrderBy(t => t.Name).ToList()
                : tokens.OrderByDescending(t => t.Name).ToList(),
            "trelloid" => isAscending 
                ? tokens.OrderBy(t => t.TrelloId).ToList()
                : tokens.OrderByDescending(t => t.TrelloId).ToList(),
            "issync" => isAscending 
                ? tokens.OrderBy(t => t.IsSync).ToList()
                : tokens.OrderByDescending(t => t.IsSync).ToList(),
            "userid" => isAscending 
                ? tokens.OrderBy(t => t.UserId).ToList()
                : tokens.OrderByDescending(t => t.UserId).ToList(),
            _ => tokens
        };
    }

    /// <summary>
    /// Apply pagination to TrelloToken list
    /// </summary>
    public List<TrelloToken> ApplyPagination(List<TrelloToken> tokens)
    {
        return tokens
            .Skip(Page * GetPageSize())
            .Take(GetPageSize())
            .ToList();
    }

    private Expression<Func<TrelloToken, bool>>? BuildTrelloTokenFilter()
    {
        var predicates = new List<Expression<Func<TrelloToken, bool>>>();

        // Filter by current user
        if (userID != null)
        {
            predicates.Add(x => x.UserId == userID);
        }

        // Filter by specific Trello token IDs if provided
        if (TrelloTokensIds != null && TrelloTokensIds.Any())
        {
            predicates.Add(x => TrelloTokensIds.Contains(x.TrelloId));
        }

        // Global search query
        if (!CommonUtils.IsEmptyString(Query))
        {
            var searchTerm = Query!.ToLower();
            predicates.Add(x => 
                (x.Name != null && x.Name.ToLower().Contains(searchTerm)) ||
                (x.TrelloId != null && x.TrelloId.ToLower().Contains(searchTerm)) ||
                (x.TrelloApiToken != null && x.TrelloApiToken.ToLower().Contains(searchTerm))
            );
        }

        // Filter by sync status if needed
        // You can add more specific filters here based on your requirements

        // Combine all predicates with AND
        if (!predicates.Any())
            return null;

        return predicates.Aggregate((expr1, expr2) => 
        {
            var parameter = Expression.Parameter(typeof(TrelloToken), "x");
            var body1 = Expression.Invoke(expr1, parameter);
            var body2 = Expression.Invoke(expr2, parameter);
            var combined = Expression.AndAlso(body1, body2);
            return Expression.Lambda<Func<TrelloToken, bool>>(combined, parameter);
        });
    }
}
