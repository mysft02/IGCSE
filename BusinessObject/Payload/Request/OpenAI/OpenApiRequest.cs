using Common.Utils;
using Microsoft.Extensions.Configuration;

namespace BusinessObject.Payload.Request.OpenAI
{
    public class OpenApiRequest
    {
        private static IConfiguration? _staticConfiguration;

        public Dictionary<string, string> Headers { get; set; } = DefaultHeaders();
        public string? CallUrl { get; set; }
        public object? Body { get; set; }

        public static Dictionary<string, string> DefaultHeaders()
        {
            var apiKey = _staticConfiguration != null 
                ? CommonUtils.GetApiKey("OPEN_API_KEY", _staticConfiguration)
                : CommonUtils.GetApiKey("OPEN_API_KEY");
            
            return new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {apiKey}" }
            };
        }
        
        public static void SetConfiguration(IConfiguration configuration)
        {
            _staticConfiguration = configuration;
        }

        public void AddHeader(string key, string value)
        {
            Headers[key] = value;
        }

        public string BuildUrl()
        {
            if (string.IsNullOrWhiteSpace(CallUrl))
                throw new ArgumentException("CallUrl is required");

            if (CallUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return CallUrl;

            return $"{CallUrl}";
        }

        public static OpenApiRequestBuilder Builder()
        {
            return new OpenApiRequestBuilder();
        }

        public class OpenApiRequestBuilder
        {
            private readonly OpenApiRequest _instance = new OpenApiRequest();

            public OpenApiRequestBuilder CallUrl(string url)
            {
                _instance.CallUrl = url;
                return this;
            }

            public OpenApiRequestBuilder Body(object body)
            {
                _instance.Body = body;
                return this;
            }

            public OpenApiRequestBuilder AddHeader(string key, string value)
            {
                _instance.AddHeader(key, value);
                return this;
            }

            public OpenApiRequest Build()
            {
                _instance.Headers ??= DefaultHeaders();
                return _instance;
            }
        }
    }
}
