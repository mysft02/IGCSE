using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSPayoutData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("referenceId")]
        public string? ReferenceId { get; set; }

        [JsonPropertyName("transactions")]
        public List<PayOSPayoutTransaction>? Transactions { get; set; }

        [JsonPropertyName("category")]
        public List<string>? Category { get; set; }

        [JsonPropertyName("approvalState")]
        public string? ApprovalState { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }
}
