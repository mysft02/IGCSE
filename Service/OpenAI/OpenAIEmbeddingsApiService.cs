using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using Common.Utils;
using Microsoft.EntityFrameworkCore;
using Repository.IRepositories;
using System.Text.Json;

namespace Service.OpenAI
{
    public class OpenAIEmbeddingsApiService
    {
        private const string EmbeddingPrefix = "SUBJECT_EMBEDDING_";
        private readonly OpenAIApiService _openAIApiService;
        private readonly IModuleRepository _moduleRepository;
        private readonly IGCSEContext _context;

        public OpenAIEmbeddingsApiService(
            OpenAIApiService openAIApiService,
            IModuleRepository moduleRepository,
            IGCSEContext context)
        {
            _openAIApiService = openAIApiService;
            _moduleRepository = moduleRepository;
            _context = context;
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

        public async Task<string> GetOrCreateSubjectEmbedding(CourseSubject subject)
        {
            var subjectName = subject.ToString();
            var cacheKey = $"{EmbeddingPrefix}{subjectName}";

            // Try to find existing module with this subject
            var existingModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.EmbeddingDataSubject == subjectName);

            if (existingModule != null)
            {
                return existingModule.EmbeddingDataSubject;
            }

            // If not found, create a new module with the subject
            var newModule = new Module
            {
                ModuleName = $"Module for {subjectName}",
                Description = $"Auto-generated module for {subjectName} subject",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmbeddingDataSubject = subjectName
            };

            await _moduleRepository.AddAsync(newModule);
            return subjectName;
        }
    }
}
