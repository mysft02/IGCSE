using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Service
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string url, IDictionary<string, string>? headers = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                ApplyHeaders(request, headers);

                _logger.LogInformation("Sending GET request to URL: {Url}", url);
                using var response = await _httpClient.SendAsync(request);
                await HandleErrorOrThrow(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during GET request to {Url}: {Message}", url, e.Message);
                throw;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, IDictionary<string, string>? headers = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = SerializeBody(body)
                };
                ApplyHeaders(request, headers);

                _logger.LogInformation("Sending POST request to URL: {Url}", url);
                using var response = await _httpClient.SendAsync(request);
                await HandleErrorOrThrow(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during POST request to {Url}: {Message}", url, e.Message);
                throw;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, IDictionary<string, string>? headers = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = SerializeBody(body)
                };
                ApplyHeaders(request, headers);

                _logger.LogInformation("Sending PUT request to URL: {Url}", url);
                using var response = await _httpClient.SendAsync(request);
                await HandleErrorOrThrow(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during PUT request to {Url}: {Message}", url, e.Message);
                throw;
            }
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest body, IDictionary<string, string>? headers = null)
        {
            try
            {
                var method = new HttpMethod("PATCH");
                using var request = new HttpRequestMessage(method, url)
                {
                    Content = SerializeBody(body)
                };
                ApplyHeaders(request, headers);

                _logger.LogInformation("Sending PATCH request to URL: {Url}", url);
                using var response = await _httpClient.SendAsync(request);
                await HandleErrorOrThrow(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during PATCH request to {Url}: {Message}", url, e.Message);
                throw;
            }
        }

        private static StringContent SerializeBody<TRequest>(TRequest body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return content;
        }

        private static void ApplyHeaders(HttpRequestMessage request, IDictionary<string, string>? headers)
        {
            if (headers == null) return;
            foreach (var kv in headers)
            {
                if (!request.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                {
                    if (request.Content != null)
                    {
                        request.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }
            }
        }

        private async Task HandleErrorOrThrow(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string message = "Error message";
                try
                {
                    message = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error reading response body: {Message}", e.Message);
                }

                _logger.LogError("Error response from API: {Message}", message);
                throw new HttpRequestException($"API call failed with status: {(int)response.StatusCode} {response.ReasonPhrase} and message: {message}");
            }
        }
    }
}


