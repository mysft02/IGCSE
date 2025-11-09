namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSPayoutApiResponse
    {
        public string? Code { get; set; }
        public string? Desc { get; set; }
        public PayOSPayoutData Data { get; set; }
    }
}
