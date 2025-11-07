using System.Threading.RateLimiting;
using BusinessObject.Model;
using BusinessObject.Payload.Request;
using BusinessObject.Payload.Response;
using Common.Utils;
using Microsoft.AspNetCore.Http;

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

    public string PrepareRequest(TrelloApiRequest request)
    {
        return PrepareRequest<object?>(request, default);
    }

    public string PrepareRequest<TBody>(TrelloApiRequest request, TBody? body = default)
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
    
    /// <summary>
    /// Download file từ URL và convert thành IFormFile để sử dụng với FileUploadHelper
    /// </summary>
    public async Task<IFormFile> DownloadToFormFileAsync(string fileUrl, TrelloToken trelloToken)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5); // Timeout cho file lớn

        try
        {
            if (fileUrl.Contains("trello.com/1"))
            {
                fileUrl = fileUrl.Replace("trello.com/1", "api.trello.com/1");
            }

            if (fileUrl.Contains("api.trello.com"))
            {
                // add token and key
                fileUrl = fileUrl + (fileUrl.Contains("?") ? "&" : "?") + $"token={trelloToken.TrelloApiToken}&key={CommonUtils.GetApiKey("TRELLO_API_KEY")}";
            }
            var response = await httpClient.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            // Không dispose stream - FormFile sẽ sử dụng stream này
            var stream = new MemoryStream(fileBytes);

            // Lấy tên file từ URL
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.AbsolutePath);
            
            // Nếu không có extension trong URL, thử lấy từ Content-Type
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                var contentType = response.Content.Headers.ContentType?.MediaType; 
                var extension = FileUploadHelper.GetExtensionFromContentType(contentType);
                fileName = $"trello_{Guid.NewGuid()}{extension}";
            }
            else
            {
                // Đảm bảo có tên file unique
                var extension = Path.GetExtension(fileName);
                var baseName = Path.GetFileNameWithoutExtension(fileName);
                fileName = $"{baseName}_{Guid.NewGuid()}{extension}";
            }

            // Tạo IFormFile từ stream
            // Note: Stream sẽ được dispose khi FormFile bị dispose, không cần using ở đây
            var formFile = new FormFile(
                stream,
                0,
                fileBytes.Length,
                "file",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
            };

            return formFile;
        }
        catch (HttpRequestException ex)
        { 
            throw new Exception($"Failed to download file from URL: {fileUrl}. Error: {ex.Message}", ex);
        }
    }
}