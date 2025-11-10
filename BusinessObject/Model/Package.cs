namespace BusinessObject.Model;

public class Package
{
    public int PackageId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool? IsActive { get; set; }

    public decimal Price { get; set; }

    public int Slot { get; set; }

    public bool? IsMockTest { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Userpackage> Userpackages { get; set; } = new List<Userpackage>();
}
