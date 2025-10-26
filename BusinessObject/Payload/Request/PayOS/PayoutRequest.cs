namespace BusinessObject.Payload.Request.PayOS
{
    public class PayoutRequest
    {
        public int? Amount { get; set; }

        public string? TeacherID { get; set; }

        public string? BankBin { get; set; }

        public string? BankAccountNumber { get; set; }
    }
}
