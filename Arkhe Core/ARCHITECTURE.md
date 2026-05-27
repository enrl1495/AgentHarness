# AgentHarness Core — Architecture

## What the Core Is

The Core is the engine. It orchestrates AI agents, persists chat history, stores vector knowledge for RAG, loads user skills, manages tools, compacts context, and indexes files. It exposes 11 interfaces. It has zero UI dependencies. It works with any `IChatClient` implementation. It compiles and runs standalone.

## Project Structure

```
AgentHarness.Core/
│
├── AgentHarness.cs              ← Orchestrator loop: receive message → history → compaction → LLM → persist
├── AgentFactory.cs              ← Creates IAgent instances via IServiceProvider (named resolution)
│
├── Agents/                      ← Specialized sub-agents (not exposed as interfaces)
│   ├── FileAnalyzerAgent.cs     ← Extracts metadata + summary from source files via LLM
│   ├── StrategyAgent.cs         ← Routes files to Semantic / BruteForce / RawVector strategy
│   ├── SvgGeneratorAgent.cs     ← Generates SVG schematics from netlist JSON + RAG context
│   └── VisionAgent.cs           ← Analyzes images (schematics, diagrams) via multimodal LLM
│
├── Compaction/                  ← Context window management
│   └── SummarizationCompactionStrategy.cs  ← Summarizes old messages when token threshold exceeded
│                                             IChatClient injected via constructor (clean interface)
│
├── Configuration/               ← Options classes (not services)
│   ├── FileSystemOptions.cs     ← Sandbox directories, UNC allowlist
│   ├── TerminalOptions.cs       ← Timeout, max output, allowed commands
│   └── WebScraperOptions.cs     ← Allowed schemes, domains, private IP blocking
│
├── Memory/                      ← Persistence layer
│   ├── ISqliteConnectionFactory.cs        ← Creates SQLite connections (returns SqliteConnection)
│   ├── SqliteConnectionFactory.cs         ← Points to %LOCALAPPDATA%/AgentHarness/agentharness.db
│   ├── SqliteChatMemoryStore.cs           ← IMemoryStore: chat history in Messages table
│   ├── SqliteVectorMemoryStore.cs         ← IVectorMemoryStore: knowledge in Knowledge table
│   └── InMemoryMemoryStore.cs             ← IMemoryStore: in-memory only, for tests/dev
│
├── Pipeline/                    ← Channel-based producer-consumer
│   └── PipelineModels.cs        ← FileAnalysisContext, FileMetadata, IndexableChunk, IndexingStrategy enum
│
├── Services/                    ← Core services (registered in DI)
│   ├── SkillRegistry.cs         ← ISkillRegistry: loads .md files from %APPDATA%/AgentHarness/skills/
│   ├── MassIndexerService.cs    ← IMassIndexerService: pipeline: producer → analyzer workers → vectorizer
│   ├── LocalEmbeddingGenerator.cs ← IEmbeddingGenerator: embeds text via ONNX model (BGE-micro-v2)
│   ├── ConfigurationService.cs  ← IConfigurationService: reads config.json + env vars
│   ├── CompatibilityMiddleware.cs ← DelegatingChatClient: DeepSeek reasoning_content formatting
│   └── DefaultToolManager.cs    ← IToolManager: flat list of registered AIFunction objects
│
└── Tools/                       ← Static tool classes (called via AIFunctionFactory)
    ├── DocumentVaultTools.cs    ← Archive files, extract PDF text/images
    ├── FileSystemTools.cs       ← Sandboxed read/list within allowed directories
    ├── SvgCircuitLibrary.cs     ← SVG <defs> for electronic components
    ├── TerminalTools.cs         ← Sandboxed process execution (dir, dotnet, git, npm...)
    ├── WebScraperTools.cs       ← HTTP GET with SSRF protection, semantic chunking
    └── WebSearchTools.cs        ← Brave Search API wrapper
```

## The 11 Interfaces (in AgentHarness.Abstractions)

| Interface | Purpose | Implemented by |
|-----------|---------|---------------|
| `IAgent` | Chat loop: ChatAsync, GetHistory, RegisterTool, ClearHistory | `AgentHarness` |
| `IAgentFactory` | Create isolated agent instances by name | `AgentFactory` |
| `IMemoryStore` | Chat history persistence (CRUD) | `SqliteChatMemoryStore`, `InMemoryMemoryStore` |
| `IVectorMemoryStore` | Vector knowledge RAG (search, save, delete) | `SqliteVectorMemoryStore` |
| `ICompactionStrategy` | Compact chat history when too large | `SummarizationCompactionStrategy` |
| `IConfigurationService` | System prompt + typed settings | `ConfigurationService` |
| `IToolManager` | Register and enumerate AI tools | `DefaultToolManager` |
| `ISkillRegistry` | Load/ toggle/ enumerate skill .md files | `SkillRegistry` |
| `IMassIndexerService` | Index directories into vector knowledge | `MassIndexerService` |
| `IDialogService` | Show messages/confirmations (host-specific) | `WinUIDialogService` (in WinUI) |
| `IDispatcherService` | Run actions on UI thread (host-specific) | `WinUIDispatcherService` (in WinUI) |

## Data Flow Inside the Core

```
User message
    │
    ▼
AgentHarness.ChatAsync()
    │
    ├─→ _configurationService.GetSystemPrompt()      ← config.json
    ├─→ _skillRegistry.LoadEnabledSkills()           ← %APPDATA%/AgentHarness/skills/*.md
    ├─→ _memoryStore.AddAsync(userMessage)           ← SqliteChatMemoryStore (Messages table)
    ├─→ _memoryStore.GetHistoryAsync()               ← full history as List<ChatMessage>
    ├─→ _compactionStrategy.CompactAsync(history)    ← SummarizationCompactionStrategy (LLM call)
    │       └─→ _memoryStore.SetHistoryAsync()       ← replace old history with compacted
    ├─→ _chatClient.GetResponseAsync(context, tools) ← IChatClient (host-provided)
    └─→ _memoryStore.AddAsync(assistantMessage)      ← persist response
```

```
Folder index request
    │
    ▼
MassIndexerService.IndexFolderAsync(path)
    │
    ├─→ [Producer] StrategyAgent.DetermineStrategyAsync() per file
    │       ├─ Semantic    → analysisChannel → FileAnalyzerAgent → vectorizationChannel
    │       ├─ BruteForce  → analysisChannel → FileAnalyzerAgent → vectorizationChannel
    │       └─ RawVector   → vectorizationChannel (skip LLM, direct to embedding)
    │
    ├─→ [Analyzer workers ×3] FileAnalyzerAgent.AnalyzeFileAsync() per chunk
    │       └─→ enriched content with summary + tags
    │
    └─→ [Vectorizer] LocalEmbeddingGenerator.GenerateAsync() per chunk
            └─→ SqliteVectorMemoryStore.SaveKnowledgeAsync() (Knowledge table)
```

## DI Composition Root (AgentHarness.Hosting)

```csharp
services.AddSingleton<ISkillRegistry, SkillRegistry>();
services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
services.AddSingleton<IMemoryStore, SqliteChatMemoryStore>();
services.AddSingleton<IVectorMemoryStore, SqliteVectorMemoryStore>();
services.AddSingleton<ICompactionStrategy, SummarizationCompactionStrategy>();
services.AddSingleton<IToolManager, DefaultToolManager>();
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddSingleton<IAgentFactory, AgentFactory>();
services.AddTransient<FileAnalyzerAgent>();
services.AddTransient<StrategyAgent>();
services.AddSingleton<IMassIndexerService, MassIndexerService>();
services.AddSingleton<IAgent>(sp => sp.GetRequiredService<IAgentFactory>().CreateAgentAsync("default").Result);
```

Hosts call `AddAgentHarnessCore()` then add their own `IChatClient` and `IEmbeddingGenerator`. Everything else is pre-wired.

## Key Architecture Decisions

| Decision | Choice | Why |
|----------|--------|-----|
| Folders, not separate csprojs | 7 folders under 1 Core project | Simpler build, same namespace isolation |
| `IVectorMemoryStore` standalone | Does NOT extend `IMemoryStore` | Separate concerns; inject both where needed, avoid casting |
| `ICompactionStrategy` decoupled | No `IChatClient` in interface; impl injects it | Interface stays pure; only the implementation knows it uses an LLM |
| Memory store split | `SqliteChatMemoryStore` + `SqliteVectorMemoryStore` | SRP: chat history ≠ vector knowledge; shared connection factory |
| `ISkillRegistry` as interface | In Abstractions, implemented in Core.Services | DI-clean; no concrete dependency in AgentHarness |
| Named agent factory via IServiceProvider | AgentFactory resolves all deps from container | Named resolution for future agent variants without code change |
| Agents as concrete classes | No `IFileAnalyzerAgent` interface | Internal pipeline detail; resolved via `IServiceProvider.GetService<T>()` |
| Hosting excludes IChatClient | `AddAgentHarnessCore()` registers everything except AI provider | Hosts configure different endpoints; Core doesn't care which provider |

## Database Schema

```
%LOCALAPPDATA%/AgentHarness/agentharness.db

Messages (SqliteChatMemoryStore)
├── Id          INTEGER PRIMARY KEY AUTOINCREMENT
├── Role        TEXT NOT NULL
├── Text        TEXT
├── RawJson     TEXT NOT NULL         ← full ChatMessage serialized
└── Timestamp   DATETIME DEFAULT CURRENT_TIMESTAMP

Knowledge (SqliteVectorMemoryStore)
├── Id          INTEGER PRIMARY KEY AUTOINCREMENT
├── Title       TEXT NOT NULL
├── Content     TEXT NOT NULL
├── Tags        TEXT DEFAULT ''
├── Embedding   BLOB                  ← float[] as byte[]
└── Timestamp   DATETIME DEFAULT CURRENT_TIMESTAMP
```

## Build

```bash
dotnet build AgentHarness.Core.csproj
# Target: net10.0, C# 14, nullable enabled, implicit usings
# Dependencies: Abstractions, Dapper, Microsoft.Data.Sqlite, Microsoft.Extensions.AI,
#               SmartComponents.LocalEmbeddings (ONNX), TiktokenSharp, UglyToad.PdfPig
```
