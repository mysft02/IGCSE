using BusinessObject.Payload.Response;
using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Request.OpenApi
{
    public class OpenAIBody : IApiBodyProcess
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("input")]
        public object Input { get; set; }

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("max_output_tokens")]
        public int? MaxTokens { get; set; }
        public IDictionary<string, IEnumerable<object>> ProcessBody()
        {

            return new Dictionary<string, IEnumerable<object>>
            {
                { "model", new object[] { Model } },
                { "temperature", new object[] { Temperature } },
                { "input", new object[] { Input } },
                { "max_output_tokens", new object[] { MaxTokens } }
            };
        }
    }

    public class OpenAIResponse
    {
        public string Id { get; set; }
        public string Model { get; set; }
        public List<OpenAIOutput> Output { get; set; } = new();
    }

    public class OpenAIOutput
    {
        public List<OpenAIContent> Content { get; set; } = new();
    }

    public class OpenAIContent
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }
}
