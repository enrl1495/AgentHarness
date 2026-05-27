using Microsoft.Extensions.AI;

namespace AgentHarness.Abstractions;

/// <summary>
/// Defines the contract for managing chat history.
/// </summary>
public interface IMemoryStore
{
    /// <summary>
    /// Adds a message to the memory store.
    /// </summary>
    Task AddAsync(ChatMessage message, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the current chat history.
    /// </summary>
    Task<List<ChatMessage>> GetHistoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Overwrites the current history with a new set of messages (useful for compaction).
    /// </summary>
    Task SetHistoryAsync(List<ChatMessage> history, CancellationToken ct = default);

    /// <summary>
    /// Clears the entire chat history.
    /// </summary>
    Task ClearAsync(CancellationToken ct = default);
}
