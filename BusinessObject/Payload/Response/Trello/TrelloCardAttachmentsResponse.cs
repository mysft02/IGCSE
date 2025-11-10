using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloCardAttachmentsResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;
    [JsonPropertyName("url")] public string Url { get; set; } = null!;
}