using Repository.IRepositories;
using System.Text.Json;

namespace Service.Assistant;

public class RAGService : IRAGService
{
    private readonly IAssistantKnowledgeRepository _knowledgeRepository;
    private readonly Service.OpenAI.OpenAIEmbeddingsApiService _embeddingsService;
    private const double MinSimilarityThreshold = 0.7;

    public RAGService(
        IAssistantKnowledgeRepository knowledgeRepository,
        Service.OpenAI.OpenAIEmbeddingsApiService embeddingsService)
    {
        _knowledgeRepository = knowledgeRepository;
        _embeddingsService = embeddingsService;
    }

    public async Task<bool> IsRelevantAsync(string userQuery, string? userId = null)
    {
        try
        {
            var relevantContexts = await RetrieveRelevantContextAsync(userQuery, userId);
            var isRelevant = relevantContexts.Any();
            Console.WriteLine($"IsRelevantAsync: Query is relevant = {isRelevant}, Found {relevantContexts.Count} contexts");
            
            // If no knowledge base data yet, still allow queries (first time setup)
            // In production, you might want to be stricter
            if (!isRelevant)
            {
                Console.WriteLine("Warning: No relevant contexts found. Allowing query anyway (knowledge base might be empty).");
            }
            
            return true; // Always allow for now - can be stricter later when knowledge base is populated
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IsRelevantAsync: {ex.Message}");
            // Allow query if error - don't block the flow
            return true;
        }
    }

    public async Task<List<RelevantContext>> RetrieveRelevantContextAsync(
        string userQuery,
        string? userId = null,
        int? courseId = null)
    {
        try
        {
            Console.WriteLine($"RAGService: Embedding query - {userQuery.Substring(0, Math.Min(50, userQuery.Length))}...");
            
            // 1. Embed query
            var queryEmbedding = await _embeddingsService.EmbedData(userQuery);
            var queryEmbeddingJson = JsonSerializer.Serialize(queryEmbedding);
            Console.WriteLine($"RAGService: Query embedded, vector length: {queryEmbedding.Count}");

            // 2. Vector search
            var knowledgeItems = await _knowledgeRepository.SearchBySimilarityAsync(
                queryEmbeddingJson,
                MinSimilarityThreshold,
                courseId,
                limit: 10
            );
            Console.WriteLine($"RAGService: Found {knowledgeItems.Count} relevant knowledge items");

            // 3. Map to RelevantContext
            return knowledgeItems.Select(k => new RelevantContext
            {
                KnowledgeId = k.KnowledgeId,
                Content = k.Content,
                SourceType = k.SourceType,
                SourceId = k.SourceId,
                SimilarityScore = 0.8 // TODO: Calculate actual similarity from SQL result
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RAGService.RetrieveRelevantContextAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Return empty list if error - don't block the flow
            return new List<RelevantContext>();
        }
    }
}

