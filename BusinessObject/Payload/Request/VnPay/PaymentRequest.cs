namespace BusinessObject.Payload.Request.VnPay
{
    public class PaymentRequest
    {
        public int CourseId { get; set; } // Chọn khóa học để thanh toán
        public decimal? Amount { get; set; }
    }
}
