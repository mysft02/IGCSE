namespace BusinessObject.Model;

public class Transactionhistory
{
    public int TransactionId { get; set; }

    public int ItemId { get; set; }

    public string UserId { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? TransactionDate { get; set; }
}
