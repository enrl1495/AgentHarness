using Microsoft.Extensions.AI;

namespace AgentHarness.Abstractions;

/// <summary>
/// Defines a technical event emitted by the agent harness.
/// </summary>
public record AgentTechnicalEvent(
    string Type,
    string Message,
    DateTime Timestamp,
    object? Data = null);

/// <summary>
/// Defines the core contract for an AI agent.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Sends a message to the agent and returns the assistant's response.
    /// </summary>
    Task<string> ChatAsync(string message, CancellationToken ct = default);

    /// <summary>
    /// Gets the current chat history asynchronously.
    /// </summary>
    Task<IEnumerable<ChatMessage>> GetHistoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Registers a tool that the agent can use.
    /// </summary>
    void RegisterTool(Delegate method, string? name = null, string? description = null);

    /// <summary>
    /// Clears the current chat history asynchronously.
    /// </summary>
    Task ClearHistoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Event triggered when a technical event occurs in the harness.
    /// </summary>
    event Action<AgentTechnicalEvent>? TechnicalEventEmitted;

    /// <summary>
    /// Gets the memory store provider used by this agent.
    /// </summary>
    IMemoryStore HistoryProvider { get; }
}
