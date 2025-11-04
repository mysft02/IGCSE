namespace BusinessObject.Model;

public class Userpackage
{
    public int PackageId { get; set; }

    public string UserId { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
