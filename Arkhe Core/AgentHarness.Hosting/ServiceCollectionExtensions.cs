using AgentHarness.Abstractions;
using AgentHarness.Core;
using AgentHarness.Core.Agents;
using AgentHarness.Core.Compaction;
using AgentHarness.Core.Memory;
using AgentHarness.Core.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentHarness.Hosting;

/// <summary>
/// Extension methods for configuring AgentHarness services in the DI container.
/// This is the shared composition root consumed by both WinUI and CLI hosts.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all core AgentHarness services to the service collection.
    /// Does NOT register IChatClient or IEmbeddingGenerator - hosts must add those separately.
    /// Note: This is a Phase 1 placeholder. Phase 2 will split memory stores and update registrations.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAgentHarnessCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Skill Registry - implements ISkillRegistry interface
        services.AddSingleton<ISkillRegistry, SkillRegistry>();

        // Memory Stores - Split into SqliteChatMemoryStore and SqliteVectorMemoryStore
        services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
        services.AddSingleton<IMemoryStore, SqliteChatMemoryStore>();
        services.AddSingleton<IVectorMemoryStore, SqliteVectorMemoryStore>();

        // Compaction Strategy - IChatClient injected via constructor
        services.AddSingleton<ICompactionStrategy, SummarizationCompactionStrategy>();

        // Specialized Agents - resolved by MassIndexerService via IServiceProvider
        services.AddTransient<FileAnalyzerAgent>();
        services.AddTransient<StrategyAgent>();

        // Tool System
        services.AddSingleton<IToolManager, DefaultToolManager>();

        // Configuration Service
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Agent Factory
        services.AddSingleton<IAgentFactory, AgentFactory>();

        // Mass Indexer Service
        services.AddSingleton<IMassIndexerService, MassIndexerService>();

        // Default Agent - created via factory for named resolution
        services.AddSingleton<IAgent>(sp =>
        {
            var factory = sp.GetRequiredService<IAgentFactory>();
            return factory.CreateAgentAsync("default").GetAwaiter().GetResult();
        });

        return services;
    }

    /// <summary>
    /// Adds an IChatClient implementation to the service collection.
    /// Hosts should call this after AddAgentHarnessCore() to configure their preferred AI provider.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="chatClientFactory">Factory function to create the chat client.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAgentHarnessChatClient(
        this IServiceCollection services,
        Func<IServiceProvider, IChatClient> chatClientFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(chatClientFactory);

        services.AddSingleton<IChatClient>(chatClientFactory);
        return services;
    }

    /// <summary>
    /// Adds an IEmbeddingGenerator implementation to the service collection.
    /// Hosts should call this after AddAgentHarnessCore() to configure their preferred embedding provider.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="embeddingGeneratorFactory">Factory function to create the embedding generator.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAgentHarnessEmbeddingGenerator(
        this IServiceCollection services,
        Func<IServiceProvider, IEmbeddingGenerator<string, Embedding<float>>> embeddingGeneratorFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(embeddingGeneratorFactory);

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(embeddingGeneratorFactory);
        return services;
    }
}
