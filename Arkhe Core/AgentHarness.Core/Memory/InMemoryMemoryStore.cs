using Microsoft.Extensions.AI;
using AgentHarness.Abstractions;

namespace AgentHarness.Core.Memory;

/// <summary>
/// A simple in-memory implementation of <see cref="IMemoryStore"/>.
/// </summary>
public class InMemoryMemoryStore : IMemoryStore
{
    private List<ChatMessage> _history = new();

    public Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        _history.Add(message);
        return Task.CompletedTask;
    }

    public Task<List<ChatMessage>> GetHistoryAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new List<ChatMessage>(_history));
    }

    public Task SetHistoryAsync(List<ChatMessage> history, CancellationToken ct = default)
    {
        _history = new List<ChatMessage>(history);
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        _history.Clear();
        return Task.CompletedTask;
    }
}
