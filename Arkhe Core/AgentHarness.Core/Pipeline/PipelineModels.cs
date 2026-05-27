using System;
using System.Collections.Generic;

namespace AgentHarness.Core.Pipeline;

public enum IndexingStrategy
{
    Semantic,
    BruteForce,
    RawVector
}

public class FileAnalysisContext
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ChunkIndex { get; set; } = 0;
    public IndexingStrategy Strategy { get; set; } = IndexingStrategy.BruteForce;
    public FileMetadata Metadata { get; set; } = new();
}

public class FileMetadata
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Entities { get; set; } = new();
    public string Language { get; set; } = "Unknown";
}

public class IndexableChunk
{
    public string SourceFile { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public IndexingStrategy Strategy { get; set; } = IndexingStrategy.BruteForce;
    public ReadOnlyMemory<float>? Vector { get; set; }
}
