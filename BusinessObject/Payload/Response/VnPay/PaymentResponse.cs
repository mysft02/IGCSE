namespace BusinessObject.Payload.Response.VnPay
{
    public class PaymentResponse
    {
        public string? PaymentUrl { get; set; }
        public string? PaymentQR { get; set; }
        // PaymentKey chỉ có khi thanh toán thành công (trong callback)
        public string? PaymentKey { get; set; }
    }
}
