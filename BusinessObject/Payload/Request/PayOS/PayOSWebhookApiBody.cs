using BusinessObject.Model;
using BusinessObject.Payload.Response;
using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Request.PayOS
{
    public class PayOSWebhookApiBody : IApiBodyProcess
    {
        [JsonPropertyName("webhookUrl")]
        public string? WebhookUrl { get; set; }

        public IDictionary<string, IEnumerable<object>> ProcessBody()
        {
            return new Dictionary<string, IEnumerable<object>>
            {
                { "webhookUrl", new object[] { WebhookUrl } },
            };
        }
    }
}
