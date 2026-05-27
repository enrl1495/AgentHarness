using Microsoft.Extensions.AI;
using AgentHarness.Abstractions;
using TiktokenSharp;

namespace AgentHarness.Core.Compaction;

/// <summary>
/// A compaction strategy that summarizes old messages using an LLM when a token threshold is reached.
/// </summary>
public class SummarizationCompactionStrategy : ICompactionStrategy
{
    private readonly int _tokenThreshold;
    private readonly string _modelName;
    private readonly IChatClient _chatClient;

    public SummarizationCompactionStrategy(IChatClient chatClient, int tokenThreshold = 1000, string modelName = "gpt-4o")
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        _chatClient = chatClient;
        _tokenThreshold = tokenThreshold;
        _modelName = modelName;
    }

    /// <summary>
    /// Compacts the history if it exceeds the token threshold.
    /// </summary>
    public async Task<List<ChatMessage>> CompactAsync(List<ChatMessage> history, CancellationToken ct = default)
    {
        var totalTokens = await Task.Run(() => CountTokens(history), ct).ConfigureAwait(false);

        if (totalTokens <= _tokenThreshold)
        {
            return history;
        }

        // Separate system messages to preserve them
        var systemMessages = history.Where(m => m.Role == ChatRole.System).ToList();
        var otherMessages = history.Where(m => m.Role != ChatRole.System).ToList();

        // Summarize the first half of the non-system messages
        int messagesToSummarizeCount = otherMessages.Count / 2;
        if (messagesToSummarizeCount <= 1)
        {
            // Not enough messages to summarize effectively, just return
            return history;
        }

        var messagesToSummarize = otherMessages.Take(messagesToSummarizeCount).ToList();
        var remainingMessages = otherMessages.Skip(messagesToSummarizeCount).ToList();

        var summary = await GenerateSummaryAsync(messagesToSummarize, ct).ConfigureAwait(false);

        var result = new List<ChatMessage>();
        result.AddRange(systemMessages);

        // Add a special message indicating this is a summary
        result.Add(new ChatMessage(ChatRole.System, $"[CONVERSATION SUMMARY]: {summary}"));
        result.AddRange(remainingMessages);

        return result;
    }

    private int CountTokens(List<ChatMessage> history)
    {
        try
        {
            // TikToken.EncodingForModel can be slow or even do I/O on first call
            var tikToken = TiktokenSharp.TikToken.EncodingForModel(_modelName);
            int count = 0;
            foreach (var message in history)
            {
                count += tikToken.Encode(message.Text ?? string.Empty).Count;
                // Approximate overhead per message for metadata (role, etc)
                count += 4;
            }
            return count;
        }
        catch
        {
            // Fallback to rough estimate if encoding fails
            return history.Sum(m => (m.Text?.Length ?? 0) / 4);
        }
    }

    private async Task<string> GenerateSummaryAsync(List<ChatMessage> messages, CancellationToken ct)
    {
        var summaryPrompt = "Please provide a concise summary of the following conversation history. Focus on key facts, decisions, and context that should be remembered for future interactions. Be brief.\n\n";

        foreach (var m in messages)
        {
            summaryPrompt += $"{m.Role}: {m.Text}\n";
        }

        var response = await _chatClient.CompleteAsync(summaryPrompt, options: null, cancellationToken: ct).ConfigureAwait(false);
        return response.Message.Text?.Trim() ?? "Summary unavailable.";
    }
}
