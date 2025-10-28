namespace BusinessObject.Model;

public class Mocktest
{
    public int MockTestId { get; set; }

    public string MockTestTitle { get; set; } = null!;

    public string MockTestDescription { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;
}
