namespace BusinessObject.DTOs.Response.Payment
{
    public class PayoutHistoryResponse
    {
        public int PayoutId { get; set; }

        public int CourseId { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
