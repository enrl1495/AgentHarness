using System;
using System.Threading;
using System.Threading.Tasks;
using AgentHarness.Core.Pipeline;
using Microsoft.Extensions.AI;

namespace AgentHarness.Core.Agents;

/// <summary>
/// Sub-agent responsible for determining the best indexing and chunking strategy for a given file.
/// </summary>
public class StrategyAgent
{
    private readonly IChatClient _chatClient;

    public StrategyAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<IndexingStrategy> DetermineStrategyAsync(string fileName, string contentPreview, CancellationToken cancellationToken = default)
    {
        var prompt = $@"You are a routing agent for a document indexer. Analyze the file name and content preview to determine the best indexing strategy.

Available strategies:
- Semantic: For structured code (C#, JS, JSON) or structured documents (Markdown, XML) where logic is split into methods, classes, or clear paragraphs. The file will be sent to an LLM analyzer.
- BruteForce: For unstructured long continuous text (books without clear formatting, essays, flat txt files) that require summarization but lack code-like structure. The file will be sent to an LLM analyzer.
- RawVector: For raw data, server logs, CSVs, binaries, minified files, or anything where a semantic summary by an LLM is a waste of time/money. This skips the LLM and goes straight to vectorization.

Respond ONLY with one of the exact three strategy names above. No quotes, no markdown, no explanation.

File Name: {fileName}
Preview (first 1000 chars):
{contentPreview}";

        try
        {
            var response = await _chatClient.CompleteAsync(prompt, options: null, cancellationToken: cancellationToken).ConfigureAwait(false);
            var strategyText = response.Message.Text?.Trim() ?? string.Empty;

            if (Enum.TryParse<IndexingStrategy>(strategyText, true, out var strategy))
            {
                return strategy;
            }

            // Fallback for structured extensions just in case the model hallucinates
            if (fileName.EndsWith(".cs") || fileName.EndsWith(".md") || fileName.EndsWith(".json") || fileName.EndsWith(".html") || fileName.EndsWith(".xml"))
                return IndexingStrategy.Semantic;

            return IndexingStrategy.BruteForce;
        }
        catch
        {
            return IndexingStrategy.BruteForce;
        }
    }
}
