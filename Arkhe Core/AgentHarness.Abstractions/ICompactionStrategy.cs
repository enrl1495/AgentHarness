using Microsoft.Extensions.AI;

namespace AgentHarness.Abstractions;

/// <summary>
/// Defines a strategy for compacting chat history when it exceeds a threshold.
/// </summary>
public interface ICompactionStrategy
{
    /// <summary>
    /// Compacts the provided chat history.
    /// </summary>
    /// <param name="history">The current history.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The compacted list of messages.</returns>
    Task<List<ChatMessage>> CompactAsync(List<ChatMessage> history, CancellationToken ct = default);
}
