using BusinessObject.Model;

namespace BusinessObject.DTOs.Response;

public class TrelloTokenResponse
{
    public string TrelloId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsSync { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // User information
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}
