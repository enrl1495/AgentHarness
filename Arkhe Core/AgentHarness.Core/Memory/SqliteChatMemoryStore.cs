using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using AgentHarness.Abstractions;
using Dapper;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AgentHarness.Core.Memory;

/// <summary>
/// SQLite implementation of <see cref="IMemoryStore"/> for chat history persistence.
/// Handles conversation message storage and retrieval only.
/// </summary>
public class SqliteChatMemoryStore : IMemoryStore, IAsyncDisposable
{
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly ILogger<SqliteChatMemoryStore> _logger;
    private SqliteConnection? _connection;

    public SqliteChatMemoryStore(ISqliteConnectionFactory connectionFactory, ILogger<SqliteChatMemoryStore> logger)
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
        _logger.LogInformation("SQLite connection opened for chat memory store.");

        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Role TEXT NOT NULL,
                Text TEXT,
                RawJson TEXT NOT NULL,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");
        _logger.LogInformation("Messages table initialized.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _logger.LogInformation("SQLite connection closed for chat memory store.");
        }
    }

    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        await _connection.ExecuteAsync("INSERT INTO Messages (Role, Text, RawJson) VALUES (@Role, @Text, @RawJson)",
            new { Role = message.Role.ToString(), Text = message.Text, RawJson = JsonSerializer.Serialize(message) });
        _logger.LogTrace("Added message to history. Role: {Role}", message.Role);
    }

    public async Task<List<ChatMessage>> GetHistoryAsync(CancellationToken ct = default)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        var rows = await _connection.QueryAsync<dynamic>("SELECT RawJson FROM Messages ORDER BY Id ASC");
        var history = rows.Select(r => JsonSerializer.Deserialize<ChatMessage>((string)r.RawJson)!).ToList();
        _logger.LogTrace("Retrieved history. Count: {Count}", history.Count);
        return history;
    }

    public async Task SetHistoryAsync(List<ChatMessage> history, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(history);

        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        using var trans = await _connection.BeginTransactionAsync(ct);
        await _connection.ExecuteAsync("DELETE FROM Messages", transaction: trans);
        foreach (var m in history)
            await _connection.ExecuteAsync("INSERT INTO Messages (Role, Text, RawJson) VALUES (@Role, @Text, @RawJson)",
                new { Role = m.Role.ToString(), Text = m.Text, RawJson = JsonSerializer.Serialize(m) }, trans);
        await trans.CommitAsync(ct);
        _logger.LogInformation("Set history. Total messages: {Count}", history.Count);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized.");

        await _connection.ExecuteAsync("DELETE FROM Messages");
        _logger.LogInformation("Cleared message history.");
    }
}
