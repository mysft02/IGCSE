using Repository.IBaseRepository;
using BusinessObject.Model;

namespace Repository.IRepositories;

public interface IAssistantKnowledgeRepository : IBaseRepository<AssistantKnowledge>
{
    Task<List<AssistantKnowledge>> GetBySourceAsync(string sourceType, int sourceId);
    Task<List<AssistantKnowledge>> SearchBySimilarityAsync(string queryEmbeddingJson, double minSimilarity = 0.7, int? courseId = null, int limit = 10);
}

