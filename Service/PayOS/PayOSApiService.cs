using BusinessObject.Payload.Request;
using BusinessObject.Payload.Request.PayOS;
using BusinessObject.Payload.Response;
using System.Threading.RateLimiting;

namespace Service.PayOS
{
    public class PayOSApiService
    {
        private const string PayOSApiBaseUrl = "https://api-merchant.payos.vn";

        // Default limiter ~10 req/sec to stay within Trello limits per token
        private readonly RateLimiter _rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 100,
            Window = TimeSpan.FromSeconds(1),
            AutoReplenishment = true
        });

        private readonly ApiService _apiService;

        public PayOSApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<T?> GetAsync<T>(PayOSApiRequest request)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req);
                return await _apiService.GetAsync<T>(url, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(PayOSApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PostAsync<TRequest, TResponse>(url, body, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(PayOSApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PutAsync<TRequest, TResponse>(url, body, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(PayOSApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PatchAsync<TRequest, TResponse>(url, body, req.Headers);
            }, request);
        }

        private async Task<T?> ExecuteWithRetry<T>(Func<PayOSApiRequest, Task<T?>> executor, PayOSApiRequest request)
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

        private string PrepareRequest(PayOSApiRequest request)
        {
            return PrepareRequest<object?>(request, default);
        }

        private string PrepareRequest<TBody>(PayOSApiRequest request, TBody? body = default)
        {
            if (body is not null)
            {
                request.Body = body;
            }

            // Normalize URL with base
            if (!string.IsNullOrWhiteSpace(request.CallUrl))
            {
                if (!request.CallUrl!.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var relative = request.CallUrl!.StartsWith("/") ? request.CallUrl! : "/" + request.CallUrl!;
                    request.CallUrl = PayOSApiBaseUrl + relative;
                }
            }

            // Process body via processor if applicable
            ProcessRequestBody(request);

            // Build final URL with params and path replacements
            return request.BuildUrl();
        }

        private void ProcessRequestBody(PayOSApiRequest request)
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
}
