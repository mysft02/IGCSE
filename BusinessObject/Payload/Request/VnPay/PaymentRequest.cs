namespace BusinessObject.Payload.Request.VnPay
{
    public class PaymentRequest
    {
        public string? UserId { get; set; }
        public decimal? Amount { get; set; }
    }
}
