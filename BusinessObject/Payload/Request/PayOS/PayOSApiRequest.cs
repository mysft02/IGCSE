using Common.Utils;

namespace BusinessObject.Payload.Request.PayOS;

public class PayOSApiRequest
{
    public static string ApiKey = CommonUtils.GetApiKey("PAYOS_API_KEY");

    public enum ResponseType
    {
        Single,
        Search
    }

    public Dictionary<string, string> Headers { get; set; } = DefaultHeaders();
    public string? CallUrl { get; set; }
    public object? Body { get; set; }
    public string? PayOSClientID { get; set; }
    public string? PayOSApiKey { get; set; }

    public Dictionary<string, string> QueryParams { get; set; } = DefaultQueryParams();
    public Type BaseType { get; set; } = typeof(object);
    public ResponseType ExpectedResponseType { get; set; } = ResponseType.Single;
    public Dictionary<string, string> PathVariables { get; set; } = new();
    public int Retry { get; set; } = 1;

    public void AddParameter(string key, string value)
    {
        QueryParams[key] = value;
    }

    public void AddPathVariable(string key, string value)
    {
        PathVariables[key] = value;
    }

    public void AddHeader(string key, string value)
    {
        Headers[key] = value;
    }

    // For dynamic deserialization decisions
    public Type GetResultType()
    {
        if (ExpectedResponseType == ResponseType.Search)
        {
            return typeof(List<>).MakeGenericType(BaseType);
        }
        return BaseType;
    }

    public string BuildUrl()
    {
        if (string.IsNullOrWhiteSpace(CallUrl))
        {
            throw new ArgumentException("CallUrl is required");
        }

        var resolved = CallUrl;
        foreach (var kv in PathVariables)
        {
            resolved = resolved.Replace("{" + kv.Key + "}", Uri.EscapeDataString(kv.Value));
        }

        var uriBuilder = new UriBuilder(resolved);
        // merge existing query with provided params
        var allParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(uriBuilder.Query))
        {
            var existing = uriBuilder.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in existing)
            {
                var parts = pair.Split('=', 2);
                var k = Uri.UnescapeDataString(parts[0]);
                var v = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                allParams[k] = v;
            }
        }
        foreach (var kv in QueryParams)
        {
            allParams[kv.Key] = kv.Value;
        }
        var keyToUse = string.IsNullOrWhiteSpace(PayOSApiKey) ? ApiKey : PayOSApiKey;
        if (!string.IsNullOrWhiteSpace(keyToUse))
        {
            allParams["key"] = keyToUse;
        }

        var queryParts = new List<string>();
        foreach (var kv in allParams)
        {
            var part = $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}";
            queryParts.Add(part);
        }
        uriBuilder.Query = string.Join("&", queryParts);
        return uriBuilder.Uri.ToString();
    }

    private static Dictionary<string, string> DefaultQueryParams()
    {
        return new Dictionary<string, string>
        {
            { "key", ApiKey }
        };
    }

    private static Dictionary<string, string> DefaultHeaders()
    {
        return new Dictionary<string, string>
        {
            { "Accept", "application/json" }
        };
    }

    public static PayOSApiRequestBuilder Builder()
    {
        return new PayOSApiRequestBuilder();
    }

    public class PayOSApiRequestBuilder
    {
        private readonly PayOSApiRequest _instance = new PayOSApiRequest();

        public PayOSApiRequestBuilder CallUrl(string url)
        {
            _instance.CallUrl = url;
            return this;
        }

        public PayOSApiRequestBuilder Body(object body)
        {
            _instance.Body = body;
            return this;
        }

        public PayOSApiRequestBuilder PayOSApiKey(string apiKey)
        {
            _instance.PayOSApiKey = apiKey;
            return this;
        }

        public PayOSApiRequestBuilder PayOSClientID(string clientID)
        {
            _instance.PayOSClientID = clientID;
            return this;
        }

        public PayOSApiRequestBuilder BaseType(Type type)
        {
            _instance.BaseType = type;
            return this;
        }

        public PayOSApiRequestBuilder ResponseType(PayOSApiRequest.ResponseType responseType)
        {
            _instance.ExpectedResponseType = responseType;
            return this;
        }

        public PayOSApiRequestBuilder Retry(int retry)
        {
            _instance.Retry = retry;
            return this;
        }

        public PayOSApiRequestBuilder AddParameter(string key, string value)
        {
            _instance.AddParameter(key, value);
            return this;
        }

        public PayOSApiRequestBuilder AddPathVariable(string key, string value)
        {
            _instance.AddPathVariable(key, value);
            return this;
        }

        public PayOSApiRequestBuilder AddHeader(string key, string value)
        {
            _instance.AddHeader(key, value);
            return this;
        }

        public PayOSApiRequest Build()
        {
            _instance.Headers ??= DefaultHeaders();
            _instance.QueryParams ??= DefaultQueryParams();
            _instance.PathVariables ??= new Dictionary<string, string>();
            _instance.BaseType ??= typeof(object);
            return _instance;
        }
    }
}