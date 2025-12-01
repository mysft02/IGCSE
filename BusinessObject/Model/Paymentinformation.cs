namespace BusinessObject.Model;

public class Paymentinformation
{
    public int PaymentInfoId { get; set; }

    public string UserId { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string BankBin { get; set; } = null!;

    public string BankAccountNumber { get; set; } = null!;
}
