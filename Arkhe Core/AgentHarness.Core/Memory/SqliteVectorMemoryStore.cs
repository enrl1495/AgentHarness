using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using AgentHarness.Abstractions;
using Dapper;
using System.Text.Json;
using System.Numerics.Tensors;
using Microsoft.Extensions.Logging;

namespace AgentHarness.Core.Memory;

/// <summary>
/// SQLite implementation of <see cref="IVectorMemoryStore"/> for vector-based knowledge storage (RAG).
/// Handles project knowledge with embeddings and semantic search only.
/// </summary>
public class SqliteVectorMemoryStore : IVectorMemoryStore, IAsyncDisposable
{
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly ILogger<SqliteVectorMemoryStore> _logger;
    private SqliteConnection? _connection;
    public string CurrentProject { get; set; } = "Global";

    public SqliteVectorMemoryStore(ISqliteConnectionFactory connectionFactory, ILogger<SqliteVectorMemoryStore> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(logger);
        
        _connectionFactory = connectionFactory;
        _logger = logger;
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _connection = _connectionFactory.CreateConnection();
        _connection.Open();
        _logger.LogInformation("SQLite connection opened for vector memory store.");

        _connection.Execute(@"
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS Knowledge (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                Tags TEXT DEFAULT '',
                Embedding BLOB,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");
        _logger.LogInformation("Knowledge table initialized with WAL mode.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _logger.LogInformation("SQLite connection closed for vector memory store.");
        }
    }

    public async Task SaveKnowledgeAsync(KnowledgeEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        var query = "INSERT INTO Knowledge (Title, Content, Tags, Embedding) VALUES (@Title, @Content, @Tags, @Embedding)";
        await _connection.ExecuteAsync(query, new
        {
            Title = entry.Title,
            Content = entry.Content,
            Tags = entry.Tags ?? "",
            Embedding = FloatArrayToBytes(entry.Vector.ToArray())
        });
        _logger.LogDebug("Saved knowledge entry: {Title}", entry.Title);
    }

    public async Task<List<KnowledgeEntry>> SearchKnowledgeAsync(ReadOnlyMemory<float> queryEmbedding, string? tagFilter = null, int limit = 3, int candidateLimit = 100, CancellationToken ct = default)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        string sql = "SELECT Id, Title, Content, Tags, Embedding FROM Knowledge";
        string? escapedTagFilter = null;
        if (!string.IsNullOrEmpty(tagFilter))
        {
            escapedTagFilter = tagFilter.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            sql += " WHERE Tags LIKE '%' || @TagFilter || '%' ESCAPE '\\'";
        }
        sql += " LIMIT @CandidateLimit";

        var entries = await _connection.QueryAsync<dynamic>(sql, new { TagFilter = escapedTagFilter, CandidateLimit = candidateLimit });

        var results = new List<(KnowledgeEntry Entry, float Similarity)>();
        var querySpan = queryEmbedding.Span;

        foreach (var row in entries)
        {
            if (row.Embedding == null) continue;

            float[] vector = BytesToFloatArray(row.Embedding);
            float similarity = TensorPrimitives.CosineSimilarity(querySpan, vector);

            results.Add((new KnowledgeEntry(row.Title, row.Content, vector, row.Tags, (int)row.Id), similarity));
        }

        _logger.LogDebug("Search completed. Found {Count} entries (candidate limit: {CandidateLimit}) for tag filter: {TagFilter}", results.Count, candidateLimit, tagFilter ?? "none");

        return results.OrderByDescending(r => r.Similarity)
                      .Take(limit)
                      .Select(r => r.Entry)
                      .ToList();
    }

    public async Task<List<KnowledgeEntry>> GetAllKnowledgeAsync(CancellationToken ct = default)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        var entries = await _connection.QueryAsync<dynamic>("SELECT Id, Title, Content, Tags, Embedding FROM Knowledge ORDER BY Id DESC");

        var results = new List<KnowledgeEntry>();
        foreach (var row in entries)
        {
            if (row.Embedding == null) continue;

            results.Add(new KnowledgeEntry(
                (string)row.Title,
                (string)row.Content,
                BytesToFloatArray((byte[])row.Embedding),
                (string)row.Tags,
                (int)row.Id));
        }
        _logger.LogDebug("Retrieved all knowledge. Total entries: {Count}", results.Count);
        return results;
    }

    public async Task UpdateKnowledgeMetadataAsync(int id, string newTitle, string newTags, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(newTitle);

        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        await _connection.ExecuteAsync("UPDATE Knowledge SET Title = @Title, Tags = @Tags WHERE Id = @Id",
            new { Title = newTitle, Tags = newTags, Id = id });
        _logger.LogInformation("Updated metadata for ID {Id}: Title='{Title}', Tags='{Tags}'", id, newTitle, newTags);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        await _connection.ExecuteAsync("DELETE FROM Knowledge WHERE Id = @Id OR Title = @Title", new { Id = int.TryParse(id, out var parsed) ? parsed : 0, Title = id });
        _logger.LogDebug("Deleted knowledge entry: {Id}", id);
    }

    private byte[] FloatArrayToBytes(float[] values)
    {
        var result = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, result, 0, result.Length);
        return result;
    }

    private float[] BytesToFloatArray(byte[] bytes)
    {
        var result = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
        return result;
    }
}
