using BusinessObject.Payload.Response;
using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Request.PayOS
{
    public class PayOSPayoutApiBody : IApiBodyProcess
    {
        [JsonPropertyName("referenceId")]
        public string? ReferenceId { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("toBin")]
        public string? ToBin { get; set; }

        [JsonPropertyName("toAccountNumber")]
        public string? ToAccountNumber { get; set; }

        public IDictionary<string, IEnumerable<object>> ProcessBody()
        {
            return new Dictionary<string, IEnumerable<object>>
            {
                { "referenceId", new object[] { ReferenceId } },
                { "amount", new object[] { Amount } },
                { "description", new object[] { Description } },
                { "toBin", new object[] { ToBin } },
                { "toAccountNumber", new object[] { ToAccountNumber } },
            };
        }
    }
}
