using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Response;
using System.Threading.RateLimiting;

namespace Service.OpenAI
{
    public class OpenAIApiService
    {
        private readonly ApiService _apiService;
        private readonly string OpenAIBaseUrl = "https://api.openai.com/v1";
        public OpenAIApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<T?> GetAsync<T>(OpenApiRequest request)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req);
                return await _apiService.GetAsync<T>(url, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(OpenApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PostAsync<TRequest, TResponse>(url, body, req.Headers);
            }, request);
        }

        private async Task<T?> ExecuteWithRetry<T>(Func<OpenApiRequest, Task<T?>> executor, OpenApiRequest request)
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
                    if (retryCount >= 2)
                    {
                        throw;
                    }
                    // Simple backoff on 429 or transient errors
                    await Task.Delay(1000);
                }
            }
        }

        private readonly RateLimiter _rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 100,
            Window = TimeSpan.FromSeconds(1),
            AutoReplenishment = true
        });

        private string PrepareRequest(OpenApiRequest request)
        {
            return PrepareRequest<object?>(request, default);
        }

        private string PrepareRequest<TBody>(OpenApiRequest request, TBody? body = default)
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
                    request.CallUrl = OpenAIBaseUrl + relative;
                }
            }


            // Build final URL with params and path replacements
            return request.BuildUrl();
        }

        private void ProcessRequestBody(OpenApiRequest request)
        {
            if (request.Body == null) return;

            if (request.Body is IApiBodyProcess processor)
            {
                var processed = processor.ProcessBody();
                request.Body = processed ?? request.Body;
                return;
            }

            throw new InvalidOperationException("Request body must implement IApiBodyProcess");
        }
    }
}
