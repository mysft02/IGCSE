namespace BusinessObject.Model;

public class Payouthistory
{
    public int PayoutId { get; set; }

    public string TeacherId { get; set; } = null!;

    public int CourseId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}
