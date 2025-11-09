namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSWebhookApiResponse
    {
        public string WebhookUrl { get; set; }

        public string AccountNumber { get; set; }

        public string AccountName { get; set; }

        public string Name { get; set; }
        
        public string ShortName { get; set; }
    }
}
