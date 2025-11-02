namespace BusinessObject.Model;

public class Transactionhistory
{
    public int TransactionId { get; set; }

    public int CourseId { get; set; }

    public string UserId { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public virtual Course Course { get; set; } = null!;
}
