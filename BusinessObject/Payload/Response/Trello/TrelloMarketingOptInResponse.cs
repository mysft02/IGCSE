using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloMarketingOptInResponse
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("optedIn")]
    public bool OptedIn { get; set; }
}
