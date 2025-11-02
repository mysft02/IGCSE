using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloListResponse
{
    [JsonPropertyName("id")] 
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")] 
    public string Name { get; set; } = null!;

    [JsonPropertyName("closed")] 
    public bool Closed { get; set; }

    [JsonPropertyName("color")] 
    public string? Color { get; set; }

    [JsonPropertyName("idBoard")] 
    public string IdBoard { get; set; } = null!;

    [JsonPropertyName("pos")] 
    public long Pos { get; set; }

    [JsonPropertyName("subscribed")] 
    public bool? Subscribed { get; set; }

    [JsonPropertyName("softLimit")] 
    public int? SoftLimit { get; set; }

    [JsonPropertyName("type")] 
    public string? Type { get; set; }

    [JsonPropertyName("datasource")] 
    public TrelloListDataSourceResponse? Datasource { get; set; }
}

public class TrelloListDataSourceResponse
{
    [JsonPropertyName("filter")] 
    public bool Filter { get; set; }
}