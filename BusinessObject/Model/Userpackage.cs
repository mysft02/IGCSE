namespace BusinessObject.Model;

public class Userpackage
{
    public int PackageId { get; set; }

    public string UserId { get; set; } = null!;

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Package Package { get; set; } = null!;
}
