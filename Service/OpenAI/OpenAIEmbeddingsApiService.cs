using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Response.OpenAI;
using Common.Utils;
using System.Text.Json;

namespace Service.OpenAI
{
    public class OpenAIEmbeddingsApiService
    {
        private readonly OpenAIApiService _openAIApiService;
        private readonly string OpenAIEmbeddingsApiBaseUrl = "https://api.openai.com/v1/embeddings";
        public OpenAIEmbeddingsApiService(OpenAIApiService openAIApiService)
        {
            _openAIApiService = openAIApiService;
        }

        public async Task<List<float>> EmbedData(Course course)
        {
            try
            {
                var inputObject = CommonUtils.ObjectToString(course);

                if (inputObject.Length > 8000)
                {
                    inputObject = inputObject.Substring(0, 8000) + "...";
                }

                var body = new OpenAIEmbeddingsBody
                {
                    Model = "text-embedding-3-small",
                    Input = inputObject,
                    EncodingFormat = "float"
                };

                var request = OpenApiRequest.Builder()
                    .CallUrl(OpenAIEmbeddingsApiBaseUrl)
                    .Body(body)
                    .Build();

                var jsonResponse = await _openAIApiService.PostAsync<object, object>(request, body);

                var jsonString = JsonSerializer.Serialize(jsonResponse);


                // Parse JSON an toàn
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                JsonElement embeddingArray;

                // ✅ Kiểm tra xem JSON là object hay array
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataProp))
                {
                    // Trường hợp thông thường của OpenAI
                    embeddingArray = dataProp[0].GetProperty("embedding");
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    // Trường hợp Swagger bạn hiển thị: mảng ở root
                    embeddingArray = root[0].GetProperty("embedding");
                }
                else
                {
                    throw new Exception("Cấu trúc JSON không hợp lệ!");
                }

                // Chuyển thành List<float>
                var embeddingList = embeddingArray
                    .EnumerateArray()
                    .Select(x => x.GetSingle())
                    .ToList();

                return embeddingList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi EmbedData: {ex.Message}");
            }
        }
    }
}
