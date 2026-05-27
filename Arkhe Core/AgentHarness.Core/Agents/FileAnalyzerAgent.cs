using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgentHarness.Core.Pipeline;
using Microsoft.Extensions.AI;

namespace AgentHarness.Core.Agents;

/// <summary>
/// A specialized sub-agent dedicated to extracting metadata and summaries from source code files.
/// </summary>
public class FileAnalyzerAgent
{
    private readonly IChatClient _chatClient;

    public FileAnalyzerAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<FileMetadata> AnalyzeFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        var prompt = $@"You are an expert code analyst. Your task is to analyze the following source code file and extract metadata.
Return ONLY a valid JSON object with this exact structure (do not include markdown formatting or backticks):
{{
  ""Summary"": ""A concise 1-2 sentence description of what this file does."",
  ""Tags"": [""tag1"", ""tag2"", ""architecture"", ""utility""],
  ""Entities"": [""MainClassName"", ""important_function"", ""ILogger""],
  ""Language"": ""C#""
}}

File: {filePath}
Content:
{content}";

        try
        {
            var response = await _chatClient.CompleteAsync(prompt, options: null, cancellationToken: cancellationToken).ConfigureAwait(false);

            var text = response.Message.Text?.Trim() ?? "{}";

            // Cleanup markdown formatting if the model ignored instructions
            if (text.StartsWith("```json")) text = text.Substring(7);
            if (text.StartsWith("```")) text = text.Substring(3);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);

            return JsonSerializer.Deserialize<FileMetadata>(text.Trim()) ?? new FileMetadata();
        }
        catch
        {
            // Fallback gracefully on parsing errors or timeouts
            return new FileMetadata
            {
                Summary = "Failed to analyze automatically.",
                Tags = new List<string> { "unparsed" },
                Language = System.IO.Path.GetExtension(filePath)
            };
        }
    }
}
