namespace Service.Assistant;

public class RelevantContext
{
    public int KnowledgeId { get; set; }
    public string Content { get; set; } = null!;
    public string? SourceType { get; set; }
    public int? SourceId { get; set; }
    public double SimilarityScore { get; set; }
}

public interface IRAGService
{
    Task<bool> IsRelevantAsync(string userQuery, string? userId = null);
    Task<List<RelevantContext>> RetrieveRelevantContextAsync(
        string userQuery,
        string? userId = null,
        int? courseId = null);
}

