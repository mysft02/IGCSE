namespace BusinessObject.Model;

public class Mocktestresult
{
    public int MockTestResultId { get; set; }

    public int MockTestId { get; set; }

    public bool IsPassed { get; set; }

    public decimal Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual Mocktest MockTest { get; set; } = null!;
}
