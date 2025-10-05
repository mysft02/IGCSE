using System.Threading.RateLimiting;
using BusinessObject.Payload.Request;
using BusinessObject.Payload.Response;

namespace Service.Trello;

public class TrelloApiService
{
    private const string TrelloApiBaseUrl = "https://api.trello.com/1";

    // Default limiter ~10 req/sec to stay within Trello limits per token
    private readonly RateLimiter _rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 100,
        Window = TimeSpan.FromSeconds(1),
        AutoReplenishment = true
    });

    private readonly ApiService _apiService;

    public TrelloApiService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<T?> GetAsync<T>(TrelloApiRequest request)
    {
        return await ExecuteWithRetry(async req =>
        {
            var url = PrepareRequest(req);
            return await _apiService.GetAsync<T>(url, req.Headers);
        }, request);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(TrelloApiRequest request, TRequest body)
    {
        return await ExecuteWithRetry(async req =>
        {
            var url = PrepareRequest(req, body);
            return await _apiService.PostAsync<TRequest, TResponse>(url, (TRequest)req.Body!, req.Headers);
        }, request);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(TrelloApiRequest request, TRequest body)
    {
        return await ExecuteWithRetry(async req =>
        {
            var url = PrepareRequest(req, body);
            return await _apiService.PutAsync<TRequest, TResponse>(url, (TRequest)req.Body!, req.Headers);
        }, request);
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(TrelloApiRequest request, TRequest body)
    {
        return await ExecuteWithRetry(async req =>
        {
            var url = PrepareRequest(req, body);
            return await _apiService.PatchAsync<TRequest, TResponse>(url, (TRequest)req.Body!, req.Headers);
        }, request);
    }

    private async Task<T?> ExecuteWithRetry<T>(Func<TrelloApiRequest, Task<T?>> executor, TrelloApiRequest request)
    {
        int retryCount = 0;
        while (true)
        {
            using var lease = await _rateLimiter.AcquireAsync(1);
            try
            {
                if (!lease.IsAcquired)
                {
                    // fallback wait if limiter queue is full
                    await Task.Delay(100);
                    continue;
                }
                return await executor(request);
            }
            catch (HttpRequestException ex)
            {
                retryCount++;
                if (retryCount >= Math.Max(1, request.Retry))
                {
                    throw;
                }
                // Simple backoff on 429 or transient errors
                await Task.Delay(1000);
            }
        }
    }

    private string PrepareRequest(TrelloApiRequest request)
    {
        return PrepareRequest<object?>(request, default);
    }

    private string PrepareRequest<TBody>(TrelloApiRequest request, TBody? body = default)
    {
        if (body is not null)
        {
            request.Body = body;
        }

        // Add token as query param if provided
        if (!string.IsNullOrWhiteSpace(request.TrelloToken))
        {
            request.AddParameter("token", request.TrelloToken!);
        }

        // Normalize URL with base
        if (!string.IsNullOrWhiteSpace(request.CallUrl))
        {
            if (!request.CallUrl!.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var relative = request.CallUrl!.StartsWith("/") ? request.CallUrl! : "/" + request.CallUrl!;
                request.CallUrl = TrelloApiBaseUrl + relative;
            }
        }

        // Process body via processor if applicable
        ProcessRequestBody(request);

        // Build final URL with params and path replacements
        return request.BuildUrl();
    }

    private void ProcessRequestBody(TrelloApiRequest request)
    {
        if (request.Body == null) return;

        if (request.Body is IApiBodyProcess processor)
        {
            request.Body = processor.ProcessBody();
            return;
        }

        throw new InvalidOperationException("Request body must implement IApiBodyProcess");
    }
}