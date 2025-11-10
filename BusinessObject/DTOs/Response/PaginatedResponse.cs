namespace BusinessObject.DTOs.Response;

/// <summary>
/// Generic paginated response for any entity type
/// </summary>
/// <typeparam name="T">The type of items in the response</typeparam>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Indicates if there are more pages available
    /// </summary>
    public bool HasNextPage => Page < TotalPages - 1;
    
    /// <summary>
    /// Indicates if there are previous pages available
    /// </summary>
    public bool HasPreviousPage => Page > 0;
    
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int CurrentPage => Page + 1;
}

