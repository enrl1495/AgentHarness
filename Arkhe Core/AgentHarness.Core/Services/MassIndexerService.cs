using AgentHarness.Abstractions;
using AgentHarness.Core.Agents;
using AgentHarness.Core.Pipeline;
using AgentHarness.Core.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Channels;

namespace AgentHarness.Core.Services;

public class MassIndexerService : IMassIndexerService
{
    private readonly IVectorMemoryStore _vectorStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;
    private readonly IServiceProvider _serviceProvider;

    public event Action<AgentTechnicalEvent>? TechnicalEventEmitted;

    public MassIndexerService(
        IVectorMemoryStore vectorStore,
        IServiceProvider serviceProvider,
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null)
    {
        _vectorStore = vectorStore;
        _serviceProvider = serviceProvider;
        _embeddingGenerator = embeddingGenerator;
    }

    private FileAnalyzerAgent? CreateAnalyzerAgent()
    {
        return _serviceProvider.GetService<FileAnalyzerAgent>();
    }

    private StrategyAgent? CreateStrategyAgent()
    {
        return _serviceProvider.GetService<StrategyAgent>();
    }

    public async Task IndexDirectoryAsync(string folderPath)
    {
        await IndexFolderAsync(folderPath);
    }

    public async Task<string> IndexFolderAsync(string folderPath, string tag = "Code")
    {
        if (!Directory.Exists(folderPath)) return $"Error: La carpeta {folderPath} no existe.";
        if (_embeddingGenerator == null) return "Error: No hay un motor de Embeddings configurado.";

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".c", ".cpp", ".h", ".cs", ".txt", ".md", ".json", ".csv", ".log" };

        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                             .Where(f => allowedExtensions.Contains(Path.GetExtension(f)))
                             .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\") && !f.Contains("\\.git\\"))
                             .ToList();

        if (files.Count == 0) return "No se encontraron archivos válidos para indexar.";

        // Configuramos los canales para el pipeline productor-consumidor
        var analysisChannel = Channel.CreateBounded<FileAnalysisContext>(new BoundedChannelOptions(10) { SingleWriter = true });
        var vectorizationChannel = Channel.CreateBounded<IndexableChunk>(new BoundedChannelOptions(20));

        // Tarea 1: Productor (El Estratega evalúa y despacha)
        var producerTask = Task.Run(async () =>
        {
            foreach (var file in files)
            {
                var content = await File.ReadAllTextAsync(file);
                var preview = content.Length > 1000 ? content.Substring(0, 1000) : content;

                var strategyAgent = CreateStrategyAgent();
                var strategy = IndexingStrategy.BruteForce;
                if (strategyAgent != null)
                {
                    strategy = await strategyAgent.DetermineStrategyAsync(Path.GetFileName(file), preview);
                }

                TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("Router", $"Archivo '{Path.GetFileName(file)}' enrutado como '{strategy}'", DateTime.Now));

                if (strategy == IndexingStrategy.RawVector)
                {
                    // Mandar directo al vectorizador, salteando el análisis LLM
                    var chunks = WebScraperTools.ChunkText(content, 1500);
                    foreach (var chunk in chunks)
                    {
                        await vectorizationChannel.Writer.WriteAsync(new IndexableChunk
                        {
                            SourceFile = file,
                            Content = chunk,
                            Tag = tag,
                            Strategy = strategy
                        });
                    }
                }
                else
                {
                    // Mandar al analista con el corte correspondiente
                    var chunks = strategy == IndexingStrategy.Semantic
                                ? WebScraperTools.SemanticChunkText(content, 6000)
                                : WebScraperTools.ChunkText(content, 6000);

                    for (int i = 0; i < chunks.Count; i++)
                    {
                        await analysisChannel.Writer.WriteAsync(new FileAnalysisContext
                        {
                            FilePath = file,
                            Content = chunks[i],
                            ChunkIndex = i,
                            Strategy = strategy
                        });
                    }
                }
            }
            analysisChannel.Writer.Complete();
        });

        // Tarea 2: Consumidores Analistas (LLM - Múltiples workers)
        var analyzerAgent = CreateAnalyzerAgent();
        int analyzerWorkers = analyzerAgent != null ? 3 : 1;
        var analyzerTasks = Enumerable.Range(0, analyzerWorkers).Select(_ => Task.Run(async () =>
        {
            await foreach (var context in analysisChannel.Reader.ReadAllAsync())
            {
                TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("Analyzer", $"Analizando fragmento {context.ChunkIndex + 1} de: {Path.GetFileName(context.FilePath)}", DateTime.Now));

                if (analyzerAgent != null)
                {
                    context.Metadata = await analyzerAgent.AnalyzeFileAsync(context.FilePath, context.Content);
                }

                var subChunks = context.Strategy == IndexingStrategy.Semantic
                                ? WebScraperTools.SemanticChunkText(context.Content, 1500)
                                : WebScraperTools.ChunkText(context.Content, 1500);

                foreach (var chunk in subChunks)
                {
                    var enrichedContent = $"File: {Path.GetFileName(context.FilePath)} (Part {context.ChunkIndex + 1})\nSummary: {context.Metadata.Summary}\nTags: {string.Join(",", context.Metadata.Tags)}\n\n{chunk}";
                    await vectorizationChannel.Writer.WriteAsync(new IndexableChunk
                    {
                        SourceFile = context.FilePath,
                        Content = enrichedContent,
                        Tag = tag,
                        Strategy = context.Strategy
                    });
                }
            }
        })).ToArray();

        // Cerrar el canal de vectorización cuando los analistas terminan
        _ = Task.WhenAll(analyzerTasks).ContinueWith(_ => vectorizationChannel.Writer.Complete());

        // Tarea 3: Consumidor Vectorizador (Aplica embeddings y persiste en DB)
        int totalVectorized = 0;
        var vectorizationTask = Task.Run(async () =>
        {
            await foreach (var chunk in vectorizationChannel.Reader.ReadAllAsync())
            {
                TechnicalEventEmitted?.Invoke(new AgentTechnicalEvent("VectorStore", $"Generando embedding para: {Path.GetFileName(chunk.SourceFile)}", DateTime.Now));
                var embedding = await _embeddingGenerator.GenerateAsync(new[] { chunk.Content });
                chunk.Vector = embedding[0].Vector;

                await _vectorStore.SaveKnowledgeAsync(new KnowledgeEntry(
                    Path.GetFileName(chunk.SourceFile),
                    chunk.Content,
                    chunk.Vector.Value,
                    chunk.Tag));

                totalVectorized++;
            }
        });

        // Esperamos que termine todo el pipeline
        await Task.WhenAll(producerTask, Task.WhenAll(analyzerTasks), vectorizationTask);

        return $"[EXCAVADORA MULTIAGENTE]: Se indexaron {files.Count} archivos y se generaron {totalVectorized} vectores enriquecidos.";
    }
}
