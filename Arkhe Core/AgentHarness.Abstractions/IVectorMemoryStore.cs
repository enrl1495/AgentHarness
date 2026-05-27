using Microsoft.Extensions.AI;

namespace AgentHarness.Abstractions;

/// <summary>
/// Defines a piece of project knowledge with its semantic vector and optional tags.
/// </summary>
public record KnowledgeEntry(
    string Title,
    string Content,
    ReadOnlyMemory<float> Vector,
    string Tags = "",
    int Id = 0);

/// <summary>
/// Standalone interface for vector-based knowledge storage (RAG).
/// Does NOT extend IMemoryStore - inject both separately where needed.
/// </summary>
public interface IVectorMemoryStore
{
    string CurrentProject { get; set; }
    Task SaveKnowledgeAsync(KnowledgeEntry entry, CancellationToken ct = default);
    Task<List<KnowledgeEntry>> SearchKnowledgeAsync(ReadOnlyMemory<float> queryEmbedding, string? tagFilter = null, int limit = 3, int candidateLimit = 100, CancellationToken ct = default);
    Task<List<KnowledgeEntry>> GetAllKnowledgeAsync(CancellationToken ct = default);
    Task UpdateKnowledgeMetadataAsync(int id, string newTitle, string newTags, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
