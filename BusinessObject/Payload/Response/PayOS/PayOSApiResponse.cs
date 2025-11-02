namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSApiResponse
    {
        public string? Code { get; set; }
        public string? Desc { get; set; }
        public PayOSData Data { get; set; }
        public string Signature { get; set; }
    }
}
