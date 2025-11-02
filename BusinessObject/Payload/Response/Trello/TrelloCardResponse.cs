using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloCardResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = null!;
    [JsonPropertyName("desc")] public string Description { get; set; } = null!;
}