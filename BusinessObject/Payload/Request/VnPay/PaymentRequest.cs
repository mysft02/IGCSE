namespace BusinessObject.Payload.Request.VnPay
{
    public class PaymentRequest
    {
        public int CourseId { get; set; }

        public decimal? Amount { get; set; }
    }
}
