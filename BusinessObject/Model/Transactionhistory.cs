namespace BusinessObject.Model;

public class Transactionhistory
{
    public int TransactionId { get; set; }

    public int CourseId { get; set; }

    public string ParentId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string VnpTxnRef { get; set; } = null!;

    public string VnpTransactionDate { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
}
