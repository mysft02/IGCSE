using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using Common.Utils;
using System.Text.Json;

namespace Service.OpenAI
{
    public class OpenAIEmbeddingsApiService
    {
        private readonly OpenAIApiService _openAIApiService;
        public OpenAIEmbeddingsApiService(OpenAIApiService openAIApiService)
        {
            _openAIApiService = openAIApiService;
        }

        public async Task<List<float>> EmbedData(Course course)
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
                .CallUrl("/embeddings")
                .Body(body)
                .Build();

            var jsonResponse = await _openAIApiService.PostAsync<object, object>(request, body);

            var jsonString = JsonSerializer.Serialize(jsonResponse);

            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            JsonElement embeddingArray;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataProp))
            {
                embeddingArray = dataProp[0].GetProperty("embedding");
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                embeddingArray = root[0].GetProperty("embedding");
            }
            else
            {
                throw new Exception("Cấu trúc JSON không hợp lệ!");
            }

            var embeddingList = embeddingArray
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToList();

            return embeddingList;
        }
    }
}
