using BusinessObject.Payload.Response;
using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Request.OpenAI
{
    public class OpenAIEmbeddingsBody : IApiBodyProcess
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("input")]
        public object Input { get; set; }

        [JsonPropertyName("encoding_format")]
        public object EncodingFormat { get; set; }

        public IDictionary<string, IEnumerable<object>> ProcessBody()
        {

            return new Dictionary<string, IEnumerable<object>>
            {
                { "model", new object[] { Model } },
                { "input", new object[] { Input } },
                { "encoding_format", new object[] { EncodingFormat } }
            };
        }
    }
}
