using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response;
using System.Threading.RateLimiting;

namespace Service.VnPay
{
    public class VnPayApiService
    {
        private readonly string BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private readonly ApiService _apiService;

        public VnPayApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        private readonly RateLimiter _rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 100,
            Window = TimeSpan.FromSeconds(1),
            AutoReplenishment = true
        });

        public async Task<T?> GetAsync<T>(VnPayApiRequest request)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req);
                return await _apiService.GetAsync<T>(url, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(VnPayApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PostAsync<TRequest, TResponse>(url, (TRequest)req.Body!, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(VnPayApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PutAsync<TRequest, TResponse>(url, (TRequest)req.Body!, req.Headers);
            }, request);
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(VnPayApiRequest request, TRequest body)
        {
            return await ExecuteWithRetry(async req =>
            {
                var url = PrepareRequest(req, body);
                return await _apiService.PatchAsync<TRequest, TResponse>(url, (TRequest)req.Body!, req.Headers);
            }, request);
        }

        private async Task<T?> ExecuteWithRetry<T>(Func<VnPayApiRequest, Task<T?>> executor, VnPayApiRequest request)

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

        private string PrepareRequest(VnPayApiRequest request)
        {
            return PrepareRequest<object?>(request, default);
        }

        private string PrepareRequest<TBody>(VnPayApiRequest request, TBody? body = default)
        {
            if (body is not null)
            {
                request.Body = body;
            }

            if (!string.IsNullOrWhiteSpace(request.BaseUrl))
            {
                if (!request.BaseUrl!.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var relative = request.BaseUrl!.StartsWith("/") ? request.BaseUrl! : "/" + request.BaseUrl!;
                    request.BaseUrl = BaseUrl + relative;
                }
            }

            // Process body via processor if applicable
            ProcessRequestBody(request);

            // Build final URL with params and path replacements
            return request.BuildVnPayUrl();
        }

        private void ProcessRequestBody(VnPayApiRequest request)
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
