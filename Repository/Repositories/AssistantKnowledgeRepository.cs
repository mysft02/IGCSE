using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using MySqlConnector;
using System.Data;

namespace Repository.Repositories;

public class AssistantKnowledgeRepository : BaseRepository<AssistantKnowledge>, IAssistantKnowledgeRepository
{
    private readonly IGCSEContext _context;

    public AssistantKnowledgeRepository(IGCSEContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<AssistantKnowledge>> GetBySourceAsync(string sourceType, int sourceId)
    {
        return await _context.Set<AssistantKnowledge>()
            .Where(k => k.SourceType == sourceType && k.SourceId == sourceId)
            .ToListAsync();
    }

    public async Task<List<AssistantKnowledge>> SearchBySimilarityAsync(
        string queryEmbeddingJson, 
        double minSimilarity = 0.7, 
        int? courseId = null, 
        int limit = 10)
    {
        var results = new List<AssistantKnowledge>();
        
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
            return results;

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                k.KnowledgeId,
                k.Content,
                k.EmbeddingData,
                k.SourceType,
                k.SourceId,
                k.Metadata,
                k.CreatedAt,
                -- Cosine similarity calculation (simplified)
                (
                    SELECT SUM(v1.value * v2.value)
                    FROM JSON_TABLE(@queryEmbedding, '$[*]' COLUMNS (value DOUBLE PATH '$', ordinal INT PATH '$')) AS v1
                    JOIN JSON_TABLE(k.EmbeddingData, '$[*]' COLUMNS (value DOUBLE PATH '$', ordinal INT PATH '$')) AS v2
                    ON v1.ordinal = v2.ordinal
                ) / (
                    SQRT(
                        (SELECT SUM(POWER(value, 2)) FROM JSON_TABLE(@queryEmbedding, '$[*]' COLUMNS (value DOUBLE PATH '$')))
                    ) * 
                    SQRT(
                        (SELECT SUM(POWER(value, 2)) FROM JSON_TABLE(k.EmbeddingData, '$[*]' COLUMNS (value DOUBLE PATH '$')))
                    )
                ) AS SimilarityScore
            FROM assistantknowledge k
            WHERE k.SourceType IN ('Course', 'Section', 'Lesson')
            AND (@courseId IS NULL OR JSON_EXTRACT(k.Metadata, '$.courseId') = @courseId)
            HAVING SimilarityScore > @minSimilarity
            ORDER BY SimilarityScore DESC
            LIMIT @limit";

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@queryEmbedding", queryEmbeddingJson);
        command.Parameters.AddWithValue("@minSimilarity", minSimilarity);
        command.Parameters.AddWithValue("@courseId", courseId.HasValue ? (object)courseId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@limit", limit);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new AssistantKnowledge
            {
                KnowledgeId = reader.GetInt32("KnowledgeId"),
                Content = reader.GetString("Content"),
                EmbeddingData = reader.GetString("EmbeddingData"),
                SourceType = reader.IsDBNull("SourceType") ? null : reader.GetString("SourceType"),
                SourceId = reader.IsDBNull("SourceId") ? null : reader.GetInt32("SourceId"),
                Metadata = reader.IsDBNull("Metadata") ? null : reader.GetString("Metadata"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            });
        }

        return results;
    }
}

