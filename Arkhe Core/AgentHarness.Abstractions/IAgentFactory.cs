using AgentHarness.Abstractions;

namespace AgentHarness.Abstractions;

/// <summary>
/// Factory interface for creating isolated agent instances.
/// Used to prevent shared state contamination (e.g., during AuditLibrary operations).
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Creates a new isolated agent instance with the specified name.
    /// </summary>
    /// <param name="name">The name of the agent to create (e.g., "default", "vision", "analyzer").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A new agent instance with isolated state.</returns>
    Task<IAgent> CreateAgentAsync(string name, CancellationToken ct = default);
}
