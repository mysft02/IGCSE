using BusinessObject.Payload.Response;
using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Request.PayOS
{
    public class PayOSApiBody : IApiBodyProcess
    {
        [JsonPropertyName("orderCode")]
        public int? OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("buyerName")]
        public string? BuyerName { get; set; }

        [JsonPropertyName("cancelUrl")]
        public string? CancelUrl { get; set; }

        [JsonPropertyName("returnUrl")]
        public string? ReturnUrl { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        public IDictionary<string, IEnumerable<object>> ProcessBody()
        {
            return new Dictionary<string, IEnumerable<object>>
            {
                { "orderCode", new object[] { OrderCode } },
                { "amount", new object[] { Amount } },
                { "description", new object[] { Description } },
                { "buyerName", new object[] { BuyerName } },
                { "cancelUrl", new object[] { CancelUrl } },
                { "returnUrl", new object[] { ReturnUrl } },
                { "signature", new object[] { Signature } },
            };
        }
    }
}
