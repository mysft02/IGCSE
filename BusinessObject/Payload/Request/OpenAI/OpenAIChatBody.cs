using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Request.OpenAI;

public class OpenAIChatBody
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "gpt-4o-mini";

    [JsonPropertyName("messages")]
    public List<OpenAIChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; } = 0.7;

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; } = 1000;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

public class OpenAIChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!; // "system", "user", "assistant"

    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}

public class OpenAIChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("choices")]
    public List<OpenAIChatChoice> Choices { get; set; } = new();
}

public class OpenAIChatChoice
{
    [JsonPropertyName("message")]
    public OpenAIChatMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public OpenAIChatMessage? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

