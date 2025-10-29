using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloLimitsResponse
{
    [JsonPropertyName("boards")]
    public TrelloBoardLimitsResponse Boards { get; set; }

    [JsonPropertyName("orgs")]
    public TrelloOrgLimitsResponse Orgs { get; set; }
}

public class TrelloBoardLimitsResponse
{
    [JsonPropertyName("totalPerMember")]
    public TrelloLimitDetailResponse TotalPerMember { get; set; }
}

public class TrelloOrgLimitsResponse
{
    [JsonPropertyName("totalPerMember")]
    public TrelloLimitDetailResponse TotalPerMember { get; set; }
}

public class TrelloLimitDetailResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("disableAt")]
    public int DisableAt { get; set; }

    [JsonPropertyName("warnAt")]
    public int WarnAt { get; set; }
}
