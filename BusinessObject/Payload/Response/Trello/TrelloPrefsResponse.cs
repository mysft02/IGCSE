using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloPrefsResponse
{
    [JsonPropertyName("privacy")]
    public TrelloPrivacyResponse Privacy { get; set; }

    [JsonPropertyName("suggestedTemplates")]
    public List<string> SuggestedTemplates { get; set; }

    [JsonPropertyName("minutesBetweenSummaries")]
    public int MinutesBetweenSummaries { get; set; }
}

public class TrelloPrivacyResponse
{
    [JsonPropertyName("fullName")]
    public string FullName { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}
