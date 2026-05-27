using AgentHarness.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentHarness.Core;

/// <summary>
/// Default implementation of <see cref="IAgentFactory"/> that creates isolated agent instances.
/// Named resolution via <see cref="IServiceProvider"/> for future agent variants.
/// </summary>
public class AgentFactory : IAgentFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentFactory>? _logger;

    public AgentFactory(
        IServiceProvider serviceProvider,
        ILogger<AgentFactory>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<IAgent> CreateAgentAsync(string name, CancellationToken ct = default)
    {
        var chatClient = _serviceProvider.GetRequiredService<IChatClient>();
        var memoryStore = _serviceProvider.GetRequiredService<IMemoryStore>();
        var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();
        var toolManager = _serviceProvider.GetRequiredService<IToolManager>();
        var skillRegistry = _serviceProvider.GetRequiredService<ISkillRegistry>();
        var compactionStrategy = _serviceProvider.GetService<ICompactionStrategy>();
        var embeddingGenerator = _serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

        var agentLogger = _serviceProvider.GetService<ILogger<AgentHarness>>();
        var agent = new AgentHarness(
            chatClient,
            memoryStore,
            configurationService,
            toolManager,
            skillRegistry,
            compactionStrategy,
            embeddingGenerator,
            agentLogger);

        return Task.FromResult<IAgent>(agent);
    }
}
