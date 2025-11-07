using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSPayoutTransaction
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("referenceId")]
        public string? ReferenceId { get; set; }

        [JsonPropertyName("amount")]
        public double? Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("toBin")]
        public string? ToBin { get; set; }

        [JsonPropertyName("toAccountNumber")]
        public string? ToAccountNumber { get; set; }

        [JsonPropertyName("toAccountName")]
        public string? ToAccountName { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("transactionDatetime")]
        public DateTime? TransactionDatetime { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
