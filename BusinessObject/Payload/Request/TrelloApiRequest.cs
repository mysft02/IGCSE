using System;
using System.Collections.Generic;

namespace BusinessObject.Payload.Request;

public class TrelloApiRequest
{
    // Default Trello API key (can be overridden via TrelloApiKey)
    public static string ApiKey = "f3e1b910789ea4d87b5eb58c76a686ab";

    public enum ResponseType
    {
        Single,
        Search
    }

    public Dictionary<string, string> Headers { get; set; } = DefaultHeaders();
    public string? CallUrl { get; set; }
    public object? Body { get; set; }
    public string? TrelloApiKey { get; set; }
    public string? TrelloToken { get; set; }

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
        var keyToUse = string.IsNullOrWhiteSpace(TrelloApiKey) ? ApiKey : TrelloApiKey;
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

    public static TrelloApiRequestBuilder Builder()
    {
        return new TrelloApiRequestBuilder();
    }

    public class TrelloApiRequestBuilder
    {
        private readonly TrelloApiRequest _instance = new TrelloApiRequest();

        public TrelloApiRequestBuilder CallUrl(string url)
        {
            _instance.CallUrl = url;
            return this;
        }

        public TrelloApiRequestBuilder Body(object body)
        {
            _instance.Body = body;
            return this;
        }

        public TrelloApiRequestBuilder TrelloApiKey(string apiKey)
        {
            _instance.TrelloApiKey = apiKey;
            return this;
        }

        public TrelloApiRequestBuilder TrelloToken(string token)
        {
            _instance.TrelloToken = token;
            return this;
        }

        public TrelloApiRequestBuilder BaseType(Type type)
        {
            _instance.BaseType = type;
            return this;
        }

        public TrelloApiRequestBuilder ResponseType(TrelloApiRequest.ResponseType responseType)
        {
            _instance.ExpectedResponseType = responseType;
            return this;
        }

        public TrelloApiRequestBuilder Retry(int retry)
        {
            _instance.Retry = retry;
            return this;
        }

        public TrelloApiRequestBuilder AddParameter(string key, string value)
        {
            _instance.AddParameter(key, value);
            return this;
        }

        public TrelloApiRequestBuilder AddPathVariable(string key, string value)
        {
            _instance.AddPathVariable(key, value);
            return this;
        }

        public TrelloApiRequestBuilder AddHeader(string key, string value)
        {
            _instance.AddHeader(key, value);
            return this;
        }

        public TrelloApiRequest Build()
        {
            _instance.Headers ??= DefaultHeaders();
            _instance.QueryParams ??= DefaultQueryParams();
            _instance.PathVariables ??= new Dictionary<string, string>();
            _instance.BaseType ??= typeof(object);
            return _instance;
        }
    }
}