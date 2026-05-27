using SmartComponents.LocalEmbeddings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgentHarness.Core.Services;

/// <summary>
/// A local, offline embedding generator that doesn't rely on external APIs.
/// Uses the BGE-micro-v2 model internally via SmartComponents.LocalEmbeddings.
/// </summary>
public class LocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>, IAsyncDisposable
{
    private readonly LocalEmbedder _embedder;
    private readonly ILogger<LocalEmbeddingGenerator> _logger;

    public LocalEmbeddingGenerator(ILogger<LocalEmbeddingGenerator> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _logger.LogInformation("Initializing local embedding generator...");
        // Esto descarga automáticamente el modelo ONNX a una cache local la primera vez que se usa.
        _embedder = new LocalEmbedder();
        _logger.LogInformation("Local embedding generator initialized.");
    }

    // Backward compatibility constructor for existing code without DI
    public LocalEmbeddingGenerator() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LocalEmbeddingGenerator>.Instance)
    {
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        var results = new List<Embedding<float>>();
        _logger.LogDebug("Generating embeddings for {Count} texts", values.Count());

        // Usamos Task.Run para no bloquear la UI, ya que la inferencia ONNX es CPU-bound
        await Task.Run(() =>
        {
            foreach (var text in values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var vector = _embedder.Embed(text);
                results.Add(new Embedding<float>(vector.Values.ToArray()));
            }
        }, cancellationToken);

        _logger.LogDebug("Generated {Count} embeddings successfully", results.Count);
        return new GeneratedEmbeddings<Embedding<float>>(results);
    }

    public EmbeddingGeneratorMetadata Metadata { get; } = new("local", new Uri("local://"), "bge-micro-v2");

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType == typeof(EmbeddingGeneratorMetadata)
            ? Metadata
            : null;
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing local embedding generator (sync dispose).");
        _embedder.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing local embedding generator (async).");
        await Task.Run(() => _embedder.Dispose());
        _logger.LogInformation("Local embedding generator disposed.");
    }
}
