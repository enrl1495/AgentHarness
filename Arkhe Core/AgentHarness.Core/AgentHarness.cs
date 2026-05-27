using Microsoft.Extensions.AI;
using AgentHarness.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AgentHarness.Core;

/// <summary>
/// Main implementation of the agent orchestration loop.
/// </summary>
public class AgentHarness : IAgent, IAsyncDisposable
{
    private readonly IChatClient _chatClient;
    private readonly IMemoryStore _memoryStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;
    private readonly ICompactionStrategy? _compactionStrategy;
    private readonly IConfigurationService _configurationService;
    private readonly IToolManager _toolManager;
    private readonly ISkillRegistry _skillRegistry;
    private readonly ILogger<AgentHarness> _logger;

    public IChatClient ChatClient => _chatClient;
    public IMemoryStore HistoryProvider => _memoryStore;

    public event Action<AgentTechnicalEvent>? TechnicalEventEmitted;

    public AgentHarness(
        IChatClient chatClient,
        IMemoryStore memoryStore,
        IConfigurationService configurationService,
        IToolManager toolManager,
        ISkillRegistry skillRegistry,
        ICompactionStrategy? compactionStrategy = null,
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null,
        ILogger<AgentHarness>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(memoryStore);
        ArgumentNullException.ThrowIfNull(configurationService);
        ArgumentNullException.ThrowIfNull(toolManager);
        ArgumentNullException.ThrowIfNull(skillRegistry);

        _chatClient = chatClient;
        _memoryStore = memoryStore;
        _configurationService = configurationService;
        _toolManager = toolManager;
        _compactionStrategy = compactionStrategy;
        _embeddingGenerator = embeddingGenerator;
        _skillRegistry = skillRegistry;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AgentHarness>.Instance;

        _logger.LogInformation("AgentHarness initialized.");
    }

    /// <summary>
    /// Registers a C# method as a tool that the agent can call.
    /// </summary>
    public void RegisterTool(Delegate method, string? name = null, string? description = null)
    {
        var tool = AIFunctionFactory.Create(method, name, description);
        _toolManager.RegisterTool(tool);
        TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("ToolRegistry", $"Registered tool: {name ?? method.Method.Name}", DateTime.Now));
    }

    public async Task<string> ChatAsync(string message, CancellationToken ct = default)
    {
        TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("Input", $"Processing user input: \"{message}\"", DateTime.Now));

        var systemPrompt = _configurationService.GetSystemPrompt();

        // Inyectar Skills Propias activas
        var activeSkills = _skillRegistry.LoadEnabledSkills();
        if (!string.IsNullOrEmpty(activeSkills))
        {
            systemPrompt += activeSkills;
        }

        // 1. Add user message to memory
        var userMessage = new ChatMessage(ChatRole.User, message);
        await _memoryStore.AddAsync(userMessage, ct).ConfigureAwait(false);

        // 2. Load history
        var history = await _memoryStore.GetHistoryAsync(ct).ConfigureAwait(false);

        // 3. Construir el contexto final
        var finalContext = new List<ChatMessage> { new ChatMessage(ChatRole.System, systemPrompt) };
        finalContext.AddRange(history);

        TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("Memory", $"Loaded {history.Count} messages + System Prompt.", DateTime.Now));

        // 4. Compaction logic
        if (_compactionStrategy != null)
        {
            var compactedHistory = await _compactionStrategy.CompactAsync(history, ct).ConfigureAwait(false);
            if (compactedHistory.Count < history.Count)
            {
                TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("Compaction", "History compacted.", DateTime.Now));
                await _memoryStore.SetHistoryAsync(compactedHistory, ct).ConfigureAwait(false);

                finalContext = new List<ChatMessage> { new ChatMessage(ChatRole.System, systemPrompt) };
                finalContext.AddRange(compactedHistory);
            }
        }

        // 5. Prepare options with registered tools
        var options = new ChatOptions { Tools = _toolManager.GetRegisteredTools().Cast<AITool>().ToList() };

        // 6. Get response from the chat client
        TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("LLM_Request", "Sending request to LLM...", DateTime.Now));

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _chatClient.CompleteAsync(finalContext, options, ct).ConfigureAwait(false);
        stopwatch.Stop();

        TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("LLM_Response", $"Received in {stopwatch.ElapsedMilliseconds:F0}ms.", DateTime.Now, response.Usage));

        // 7. Update memory with the assistant's response
        var assistantMessage = response.Message;
        await _memoryStore.AddAsync(assistantMessage, ct).ConfigureAwait(false);

        return assistantMessage.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the current chat history from the memory store asynchronously.
    /// </summary>
    public async Task<IEnumerable<ChatMessage>> GetHistoryAsync(CancellationToken ct = default)
    {
        return await _memoryStore.GetHistoryAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Clears the current chat history asynchronously.
    /// </summary>
    public async Task ClearHistoryAsync(CancellationToken ct = default)
    {
        await _memoryStore.ClearAsync(ct).ConfigureAwait(false);
        TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("Memory", "Chat history cleared.", DateTime.Now));
    }

    // Removed the blocking History property

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing AgentHarness...");

        if (_embeddingGenerator is IAsyncDisposable asyncEmbedding)
        {
            await asyncEmbedding.DisposeAsync();
            _logger.LogDebug("Embedding generator disposed asynchronously.");
        }
        else if (_embeddingGenerator is IDisposable syncEmbedding)
        {
            syncEmbedding.Dispose();
            _logger.LogDebug("Embedding generator disposed synchronously.");
        }

        if (_memoryStore is IAsyncDisposable asyncMemory)
        {
            await asyncMemory.DisposeAsync();
            _logger.LogDebug("Memory store disposed asynchronously.");
        }

        _logger.LogInformation("AgentHarness disposed.");
    }
}
