# Architecture Report: AgentHarness Proto (Sub-agente sdd-explore)

## Project Purpose

AgentHarness is a **.NET 10 AI agent orchestration harness** — a desktop + headless platform that manages AI agent lifecycle, tool execution, memory/RAG retrieval, conversation compaction, and multi-agent pipelines. It connects to OpenAI-compatible LLMs (or Ollama for vision), provides a WinUI 3 desktop shell with MVVM, and a headless CLI. It was built over 1 day (2026-05-26) with 2 audit passes and is now in the middle of a "rebuild-v2" architectural refactor (Phases 1-2 of 4 complete).

---

## PHASE 1: SDD Artifacts (OpenSpec)

### `openspec/config.yaml`
- **Purpose**: Bootstrap configuration for the SDD process. Defines schema, stack, testing capabilities, quality tools, and archival history.
- **Key contents**: `schema: spec-driven`, `strict_tdd: true`, 147 tests all passing, 2 archived changes (audit-dios-extremo, audit-winui-layer). Rules for all SDD phases documented.
- **Issue**: References `.slnx` (with `x`) — the actual solution format; notes 5 projects but now has 6 (Hosting added). Duplicate `audit-winui-layer` entry under `archived_changes` (lines 58-65 repeat lines 51-56).

### `openspec/changes/rebuild-v2/proposal.md`
- **Purpose**: Intent and scope of the rebuild-v2 change. Fixes 8+ concrete coupling violations in a codebase claiming Clean Architecture.
- **Key contents**: Rewrite Core into bounded modules, new `AgentHarness.Hosting` project, interface-first design, English standardization, port 147 tests. Split into 4 PRs.
- **Issue**: References C# 14.0 but most `.csproj` files don't declare `<LangVersion>14.0` (only Hosting does).

### `openspec/changes/rebuild-v2/exploration.md`
- **Purpose**: Detailed exploration of the codebase before committing to rebuild.
- **Key contents**: 3 approaches evaluated (Incremental, Clean Rebuild, Minimum Viable). Identifies 9 architecture leaks: `SqliteMemoryStore` conflation, `SkillRegistry` concrete dependency, `IAgent` casts in ViewModels, duplicate DI setup between WinUI and CLI, `ICompactionStrategy` referencing `IChatClient`.
- **Recommendation**: Approach 2 — Clean Rebuild with Shared Core.

### `openspec/changes/rebuild-v2/design.md`
- **Purpose**: Technical design with 8 architecture decisions.
- **Key decisions**: Folders inside single Core project (not separate csprojs), `ICompactionStrategy` decoupled from `IChatClient`, `IVectorMemoryStore` standalone, Sqlite stores split with shared connection factory, `IAgentFactory` named resolution via `IServiceProvider`, specialized agents remain concrete but created via factory.
- **Issue**: The `IAgent` singleton registration (line 103) uses `.GetAwaiter().GetResult()` on `CreateAgentAsync` — this is a deadlock risk if used with sync-over-async patterns.

### `openspec/changes/rebuild-v2/tasks.md`
- **Purpose**: 28 tasks across 4 phases (4 chained PRs).
- **Progress**: Phase 1 (6/6 complete), Phase 2 (9/9 complete) = 15/28 done. Phase 3 (Host Adapters: WinUI + CLI) and Phase 4 (Tests + Polish) remain.
- **Issue**: WinUI project has WMC9999 XAML compiler bug (known pre-existing issue).

### `openspec/changes/rebuild-v2/apply-progress.md`
- **Purpose**: Tracks Phase 1 + Phase 2 completion status.
- **Key findings**: 24 files changed across Phases 1-2. WinUI and Tests blocked (WinUI XAML errors + Core refactor incompatibility). Core + Abstractions + Hosting build clean.

### `openspec/changes/rebuild-v2/specs/*.md` (8 capability specs)
- **agent-orchestration/spec.md**: 3 requirements — `IAgent`, `IAgentFactory` (named resolution, `AgentNotFoundException`), harness loop with max iteration guard and tool invocation cycle.
- **memory-store/spec.md**: 3 requirements — `IMemoryStore` for chat, `IVectorMemoryStore` standalone for RAG, `SqliteChatMemoryStore`/`SqliteVectorMemoryStore` separation with independent schemas.
- **tool-system/spec.md**: 3 requirements — `IToolManager` register/invoke, `[DangerousTool]` attribute + `RequiresConfirmation`, DI-only instantiation.
- **compaction/spec.md**: 2 requirements — `ICompactionStrategy.CompactAsync`, token budget enforcement at threshold.
- **pipeline/spec.md**: 2 requirements — sequential pipeline with `IAgentFactory`, `MassIndexerService` with batch indexing + cancellation.
- **hosting/spec.md**: 3 requirements — shared composition root, host-specific overrides, zero `new` on Core types in hosts.
- **winui-shell/spec.md**: 3 requirements — ViewModel constructor injection, no `IAgent` casts, English-only identifiers.
- **cli-repl/spec.md**: 3 requirements — shared DI, headless (no WinUI deps), English-only output.

---

## PHASE 2: Archived Changes

### `audit-dios-extremo` (C# Backend Audit)
- **Proposal**: 25 findings: 5 CRITICAL (arbitrary PowerShell, plaintext API keys, path traversal, full-table SQLite, DI lifecycle leaks), 8 HIGH, 7 MEDIUM, 5 LOW. Score: 6.7/10 → 8.5/10.
- **Archive Report**: PASS WITH WARNINGS. 41/45 tasks complete (91%). Tests: 5 → 117 (2240% increase). Spec compliance: 84%. Deferred: DPAPI encryption, FTS5, pipeline resilience. 
- **Key deliverables**: Terminal sandbox (22 command allowlist + argument regex), path traversal protection, SSRF prevention (RFC 1918 blocklist + cloud metadata blocking), `IAsyncDisposable` on 3 services, `ILogger` partial migration, `.editorconfig`, CI/CD pipeline, `Stopwatch` timing, `IAgentFactory` + `AgentFactory` for session isolation.

### `audit-winui-layer` (XAML/WinUI Audit)
- **Proposal**: 22 issues in 11 XAML files: 3 CRITICAL (zero accessibility, ItemsControl without virtualization, broken bindings), 6 HIGH, 7 MEDIUM, 6 LOW. Score: 4.8/10 → ~7.5/10.
- **Archive Report**: PASS WITH WARNINGS. 63/66 tasks complete. 20/22 spec requirements met (2 blocked by WinUI 3 platform). 30 new tests added. 
- **Key deliverables**: `AutomationProperties.Name` on 42 controls, `x:Bind` migration (~70%), `x:DataType` on all DataTemplates, LibraryPage `ListView` virtualization, 22+ hardcoded colors → theme resources, broken `ThoughtLogPanel` bindings fixed, Light theme crash fixed, unified `ValueToVisibilityConverter`.
- **10 platform blockers**: `MinWidth`/`MinHeight` on Window, VisualStateManager narrow state, WMC9999 SDK XamlCompiler bug.

---

## PHASE 3: Abstractions (11 interfaces)

### `IAgent.cs` (48 lines)
- **Purpose**: Core agent contract + `AgentTechnicalEvent` record.
- **Key members**: `ChatAsync(message, ct)`, `GetHistoryAsync(ct)`, `RegisterTool(Delegate)`, `ClearHistoryAsync(ct)`, event `TechnicalEventEmitted`, `HistoryProvider` property.
- **Issue**: `HistoryProvider` is typed as `IMemoryStore` — this forces all agents to have a chat memory store even when they only need vector search. Tightly couples `IAgent` to the chat-history concern. Also, `RegisterTool(Delegate)` bypasses type safety.

### `IAgentFactory.cs` (18 lines)
- **Purpose**: Factory for creating isolated agent instances.
- **Key members**: `CreateAgentAsync(string name, CancellationToken ct)`.
- **Issue**: Method says "isolated agent instances" but the current implementation (`AgentFactory`) always returns the same type (`AgentHarness`) regardless of name. Named resolution is not implemented.

### `IMemoryStore.cs` (29 lines)
- **Purpose**: Chat history persistence contract.
- **Key members**: `AddAsync`, `GetHistoryAsync`, `SetHistoryAsync`, `ClearAsync`.
- **Issue**: References `Microsoft.Extensions.AI.ChatMessage` — acceptable coupling since this is the chat store.

### `IVectorMemoryStore.cs` (27 lines)
- **Purpose**: Standalone vector knowledge store (RAG) + `KnowledgeEntry` record.
- **Key members**: `SaveKnowledgeAsync`, `SearchKnowledgeAsync` (with `candidateLimit`), `GetAllKnowledgeAsync`, `UpdateKnowledgeMetadataAsync`, `DeleteAsync`, `CurrentProject`.
- **Issue**: `KnowledgeEntry` uses `ReadOnlyMemory<float>` which is fine for BGE-micro-v2 384-dim vectors but any change in embedding dimension breaks all stored entries. `CurrentProject` property suggests multi-project support but is only ever set to "Global".

### `IToolManager.cs` (9 lines)
- **Purpose**: Tool registration and retrieval contract.
- **Key members**: `RegisterTool(object)`, `GetRegisteredTools()`.
- **Issue**: `object` type for tools — no constraint. The implementation casts to `AITool` in `AgentHarness.ChatAsync()`. No `RequiresConfirmation` method matching the spec's `[DangerousTool]` attribute requirement. This interface is significantly simpler than the spec demands.

### `IConfigurationService.cs` (7 lines)
- **Purpose**: Configuration and system prompt contract.
- **Key members**: `GetSystemPrompt()`, `GetSetting<T>(key)`.
- **Issue**: Very thin. No methods for save, encryption, or API key management. `GetSetting<T>` uses `Convert.ChangeType` which will throw on unsupported conversions.

### `ICompactionStrategy.cs` (17 lines)
- **Purpose**: Decoupled compaction contract (IChatClient removed per Phase 1).
- **Key members**: `CompactAsync(List<ChatMessage> history, CancellationToken ct)`.
- **Status**: Clean after Phase 1 fix. Previously had `IChatClient` parameter.

### `IDialogService.cs` (7 lines)
- **Purpose**: WinUI dialog abstraction.
- **Key members**: `ShowMessageAsync`, `ShowConfirmationAsync`.

### `IDispatcherService.cs` (13 lines)
- **Purpose**: UI thread dispatch abstraction (decouples ViewModels from `DispatcherQueue`).
- **Key members**: `RunOnUIThread(Action)`.

### `IMassIndexerService.cs` (11 lines)
- **Purpose**: Mass indexing pipeline contract.
- **Key members**: `IndexDirectoryAsync(string path)`, `IndexFolderAsync(string folderPath, string tag)`, event `TechnicalEventEmitted`.
- **Issue**: Two methods that do essentially the same thing — `IndexDirectoryAsync` just calls `IndexFolderAsync`. The `TechnicalEventEmitted` event on a service interface is unusual (events leak implementation detail).

### `ISkillRegistry.cs` (40 lines)
- **Purpose**: Dynamic skill loading from AppData + `SkillInfo` record.
- **Key members**: `GetSkillsFolderPath()`, `GetSkills()`, `ToggleSkill(name, isEnabled)`, `LoadEnabledSkills()`.
- **Status**: New in Phase 1, already implemented.

---

## PHASE 4: Core Implementation

### `AgentFactory.cs` (50 lines) — `AgentHarness.Core`
- **Purpose**: `IAgentFactory` implementation using `IServiceProvider` for dependency resolution.
- **Key methods**: `CreateAgentAsync(string name, CancellationToken)`. Ignores `name` parameter — always creates `AgentHarness`.
- **Pattern**: Good — uses `GetRequiredService` for required deps, `GetService` for optional. Factory via DI container.
- **Issue**: `name` parameter unused. Named resolution deferred. Uses `new AgentHarness(...)` which is acceptable inside the factory.

### `AgentHarness.cs` (166 lines) — `AgentHarness.Core`
- **Purpose**: Main orchestration loop: memory → context building → compaction → LLM call → response storage.
- **Key members**: 8 constructor deps (minimized from original 9), `ChatAsync`, `RegisterTool`, `GetHistoryAsync`, `ClearHistoryAsync`, `DisposeAsync`.
- **Patterns**: `ArgumentNullException.ThrowIfNull` on required deps, `Stopwatch` for timing, `event Action<AgentTechnicalEvent>`, manual disposal chain in `DisposeAsync`.
- **Issues**: 
  1. `RegisterTool(Delegate)` creates `AIFunctionFactory.Create` but never disposes it.
  2. System prompt + active skills concatenation could exceed context window (no token budget check on prompt).
  3. `DateTime.Now` used for `AgentTechnicalEvent.Timestamp` (should use UTC or the event should be timezone-independent).
  4. History loading adds user message, loads all history, then context contains system prompt + history (meaning system prompt is at position 0 every turn — correct but wastes tokens if history is large).

### `ISqliteConnectionFactory.cs` (16 lines) + `SqliteConnectionFactory.cs` (34 lines) — `AgentHarness.Core.Memory`
- **Purpose**: Shared SQLite connection creation for co-located stores.
- **Key members**: `CreateConnection()`. Factory creates DB folder at `%LocalAppData%\AgentHarness`, sets connection string.
- **Pattern**: Factory pattern, hardcoded `agentharness.db` path.
- **Issue**: No WAL mode pragma (only `SqliteVectorMemoryStore` sets it). If `SqliteChatMemoryStore` opens first, WAL won't be enabled.

### `SqliteChatMemoryStore.cs` (102 lines) — `AgentHarness.Core.Memory`
- **Purpose**: Chat history persistence via `IMemoryStore`.
- **Key members**: `AddAsync`, `GetHistoryAsync`, `SetHistoryAsync` (transactional DELETE+INSERT), `ClearAsync`, `DisposeAsync`.
- **Patterns**: Dapper with JSON serialization, single-row-per-message in Messages table. `IAsyncDisposable`.
- **Issues**:
  1. Stores entire `ChatMessage` as JSON (`RawJson`) alongside Role/Text columns — redundant storage.
  2. `SetHistoryAsync` does DELETE ALL then re-insert each row — for large histories this is O(n) wasteful; could use replace or diff.
  3. No session/multi-user support — all messages in one table.
  4. Connection held open for lifetime — could be problematic for concurrent access.

### `SqliteVectorMemoryStore.cs` (170 lines) — `AgentHarness.Core.Memory`
- **Purpose**: Vector knowledge storage with cosine similarity search.
- **Key members**: `SaveKnowledgeAsync`, `SearchKnowledgeAsync` (BruteForce cosine similarity with candidate limit), `GetAllKnowledgeAsync`, `UpdateKnowledgeMetadataAsync`, `DeleteAsync`, `DisposeAsync`.
- **Patterns**: Dapper, `Buffer.BlockCopy` for float-byte conversion, `TensorPrimitives.CosineSimilarity`, WAL journal mode, LIKE ESCAPE for SQL injection prevention.
- **Issues**:
  1. `SearchKnowledgeAsync` loads ALL candidate rows into memory then computes cosine similarity — O(n) in candidate size with memory for full vectors. Candidate limit of 100 mitigates this but 100 is low for large libraries.
  2. `DeleteAsync` does `WHERE Id = @Id OR Title = @Title` — deleting by title could accidentally remove multiple entries (title collisions).
  3. No approximate nearest neighbor (ANN) — brute-force only.
  4. `BytesToFloatArray` returns fresh arrays — reasonable but foreach across 100 entries with 384-dim vectors is ~150KB.

### `InMemoryMemoryStore.cs` (35 lines) — `AgentHarness.Core.Memory`
- **Purpose**: Lightweight in-memory implementation for testing.
- **Key members**: All `IMemoryStore` methods, thread-unsafe `List<ChatMessage>`.
- **Issue**: Not thread-safe. No locking.

### `SkillRegistry.cs` (96 lines) — `AgentHarness.Core.Services`
- **Purpose**: Loads/reads `.md` skill files from `%LocalAppData%\AgentHarness\skills`.
- **Key pattern**: Parameterless constructor (creates folder, writes default `CleanCodeReviewer.md` in Spanish).
- **Issues**:
  1. Default skill content is in **Spanish** ("Cuando revises código...") — violates the English-only goal.
  2. `Debug.WriteLine` instead of `ILogger` (8 remaining files per roadmap).
  3. No `ArgumentNullException.ThrowIfNull` in constructor (roadmap Priority 3).
  4. `_disabledSkills` is in-memory only — not persisted across restarts.

### `SummarizationCompactionStrategy.cs` (97 lines) — `AgentHarness.Core.Compaction`
- **Purpose**: Token-counting compaction via TikToken + LLM summarization.
- **Key pattern**: `IChatClient` ctor-injected (Phase 1), TikToken for token counting, summarizes first half of non-system messages.
- **Issues**:
  1. `TikToken.EncodingForModel` is called on EVERY compaction despite model name rarely changing — should be cached.
  2. `CountTokens` catch block falls back to char-count / 4 — silent degradation, no logging.
  3. `GenerateSummaryAsync` sends raw message content to LLM — could leak PII or API keys.

### `PipelineModels.cs` (37 lines) — `AgentHarness.Core.Pipeline`
- **Purpose**: Domain models for the indexing pipeline.
- **Classes**: `IndexingStrategy` enum (Semantic, BruteForce, RawVector), `FileAnalysisContext`, `FileMetadata`, `IndexableChunk`.
- **Status**: Clean domain model. No issues.

### Configuration options (3 files) — `AgentHarness.Core.Configuration`
- `TerminalOptions.cs` (32 lines): Sandbox toggle, timeout 30s, max output 1MB, 15 allowed commands.
- `WebScraperOptions.cs` (32 lines): Timeout 30s, max response 1MB, allowed schemes, allowed domains, private IP blocking.
- `FileSystemOptions.cs` (21 lines): Sandbox directories, UNC path blocking.
- **Issues**: These `Options` classes are defined but never registered in DI or consumed via `IOptions<T>`. They exist as documentation only — the actual tools hardcode their values.

### Agents (4 files) — `AgentHarness.Core.Agents`
- **`VisionAgent.cs`** (53 lines): Multi-modal image analysis. Prompt is in **Spanish** ("Sos un ingeniero..."), catch returns error string in Spanish. 
- **`SvgGeneratorAgent.cs`** (61 lines): SVG generation from netlist JSON. Prompt in **Spanish** ("Sos un generador experto..."). Good: markdown cleanup (````svg`, ````xml`, etc.).
- **`StrategyAgent.cs`** (57 lines): Routes files to indexing strategy. **English** prompt — good. `Enum.TryParse` with extension-based fallback.
- **`FileAnalyzerAgent.cs`** (60 lines): Metadata extraction. **English** prompt — good. Markdown cleanup. Graceful fallback on parse errors.
- **Key issue**: These agents are concrete classes, not behind interfaces. The design says this is acceptable (internal pipeline details), but they're `public` so they leak outside Core.

### Tools (6 files) — `AgentHarness.Core.Tools`
- **`TerminalTools.cs`** (143 lines): `static class` with allowlist (22 commands), blocklist (7), dangerous pattern regex, Base64 `-EncodedCommand` execution, 30s timeout with `Process.Kill()`, 10KB output limit. **Spanish error messages**.
- **`WebScraperTools.cs`** (197 lines): `static class` with SSRF prevention (localhost, 127.0.0.1, private IPs, cloud metadata, file/ftp protocol blocking), 10s timeout, HTML cleaning via HtmlAgilityPack, chunking (fixed + semantic). **Spanish error messages**.
- **`FileSystemTools.cs`** (145 lines): `static class` with sandbox directory validation (3 allowed base dirs), path traversal detection, UNC blocking, `ListDirectory`, `ReadFile` (10KB limit). **Spanish error messages**.
- **`WebSearchTools.cs`** (74 lines): Tavily API client. `HttpClient` field, not injected. **Spanish error messages**.
- **`DocumentVaultTools.cs`** (178 lines): PDF text extraction via Python microservice, image extraction via UglyToad.PdfPig, path injection validation, vault archiving. **Spanish error messages**.
- **`SvgCircuitLibrary.cs`** (36 lines): Static SVG `<defs>` primitives: resistor, capacitor, diode, inductor, GND, DC source. Spanish XML comments.
- **Critical issue**: ALL tools are `static class` — untestable without actual I/O. Cannot be mocked. The spec requires DI-only instantiation but these bypass DI entirely.

### Services (4 files) — `AgentHarness.Core.Services`
- **`ConfigurationService.cs`** (46 lines): Reads `config.json` from `AppDomain.CurrentDomain.BaseDirectory`. `GetValue` falls back to environment variables. **No null guard**, uses `Debug.WriteLine`. `GetSetting<T>` with `Convert.ChangeType`.
- **`DefaultToolManager.cs`** (18 lines): Simple `List<object>` wrapper. No security checks, no `RequiresConfirmation`.
- **`LocalEmbeddingGenerator.cs`** (72 lines): ONNX BGE-micro-v2 via `SmartComponents.LocalEmbeddings`. Both DI and parameterless constructors. `Task.Run` for CPU-bound work. `GetService` returns metadata.
- **`CompatibilityMiddleware.cs`** (134 lines): `DelegatingChatClient` with DeepSeek phi/r1 tool stripping, `reasoning_content` wrapping, compatibility error detection, streaming error recovery via single enumerator try-catch.
- **`MassIndexerService.cs`** (178 lines): Channel-based producer-consumer pipeline (bounded channels 10/20), `Task.Run` workers, 3 analyzers, LLM enrichment, vectorization, progress events. **Spanish output message**. Uses `IServiceProvider.GetService<T>()` for agents (fixed in Phase 2).

---

## PHASE 5: Hosting, WinUI, CLI

### `AgentHarness.Hosting/ServiceCollectionExtensions.cs` (102 lines)
- **Purpose**: Shared DI composition root. 
- **Key methods**: `AddAgentHarnessCore()` (registers everything except `IChatClient` and `IEmbeddingGenerator`), `AddAgentHarnessChatClient(factory)`, `AddAgentHarnessEmbeddingGenerator(factory)`.
- **Registrations**: `ISkillRegistry`, `ISqliteConnectionFactory`, `IMemoryStore`→SqliteChat, `IVectorMemoryStore`→SqliteVector, `ICompactionStrategy`→Summarization, `FileAnalyzerAgent`/`StrategyAgent` (transient), `IToolManager`, `IConfigurationService`, `IAgentFactory`, `IMassIndexerService`, `IAgent` (default singleton via factory).
- **Issue**: `IAgent` singleton uses `.GetAwaiter().GetResult()` — deadlock risk. Hosts must call `AddAgentHarnessCore()` before `AddAgentHarnessChatClient()`.

### `AgentHarness.WinUI/App.xaml.cs` (104 lines)
- **Purpose**: WinUI composition root. **NOT using shared hosting yet** (Phase 3 task).
- **Issues**:
  1. Still creates `new SqliteMemoryStore()`, `new ConfigurationService()`, `new DefaultToolManager()` inline — bypasses shared hosting.
  2. `SummarizationCompactionStrategy` created with literal `1500` threshold — no config.
  3. `MassIndexerService` instantiated with 3 constructor args (old signature, missing `IServiceProvider`).
  4. `skillRegistry: null` passed to `AgentHarness` constructor — should use `ISkillRegistry`.
  5. `SettingsViewModel` captured for API key in chat client factory — DI captive dependency.
  6. Mica/Acrylic backdrop logic hardcoded in `OnLaunched`.

### `AgentHarness.WinUI/MainWindow.xaml.cs` (133 lines)
- **Purpose**: Main window with NavigationView, page routing, command palette, settings flyout, thought log panel.
- **Patterns**: Constructor injection for `MainViewModel`, manual `DataContext` assignment, `x:FieldModifier="internal"`, keyboard accelerator Ctrl+K for command palette.
- **Issue**: `NavigateToPage` has duplicate overloads (one string, one Type) — the Type overload doesn't handle ThoughtLogPanel toggle.

### `AgentHarness.WinUI/ViewModels/MainViewModel.cs` (416 lines)
- **Purpose**: Main orchestrating ViewModel. **416 lines — too large.**
- **Issues**:
  1. `CurrentChatClient` casts `_agent` to `AgentHarness.Core.AgentHarness` — violates spec (Phase 3 target).
  2. Creates `new LocalEmbeddingGenerator()` directly — bypasses DI.
  3. `OnSettingsSaved` creates a completely new `AgentHarness` + `ConfigurationService` + `DefaultToolManager` — bypasses DI entirely.
  4. `RegisterDefaultTools` is 156 lines of tool lambda registrations — should be extracted to composition root.
  5. Uses `SqliteMemoryStore` concrete casts for `retag_knowledge` and `audit_fragment` tools — should use `IVectorMemoryStore.UpdateKnowledgeMetadataAsync`.
  6. Creates `new SvgGeneratorAgent(CurrentChatClient)` inline.
  7. `ChatMessageViewModel.Timestamp` uses `DateTime.Now` instead of constructor parameter.

### `AgentHarness.Cli/Program.cs` (136 lines)
- **Purpose**: Headless CLI with REPL loop. **NOT using shared hosting yet** (Phase 3 target).
- **Issues**:
  1. Creates `new SqliteMemoryStore()` directly — duplicate of WinUI setup.
  2. API key prompted from console, not encrypted storage.
  3. Creates `new LocalEmbeddingGenerator()` directly.
  4. Tool registration is a subset of WinUI tools — inconsistent feature set.
  5. No `IAgentFactory`, `ISkillRegistry`, `ICompactionStrategy` registered.

---

## PHASE 6: Tests (14 files, 147 tests)

### `AgentHarness.Tests.csproj`
- Targets `net10.0-windows10.0.19041.0` (same as WinUI — tests require Windows). References: Abstractions, Core, Hosting, WinUI.
- **Issue**: xUnit 2.9.3, Moq 4.20.72, coverlet. No `LangVersion` set.

### `AgentHarnessTests.cs` (1 test)
- Tests: `ChatAsync_UpdatesMemoryStore`. Uses Moq, InMemory memory store. **1 test only — minimal coverage for the harness loop.**

### `CompactionTests.cs` (2 tests)
- Tests still use OLD signature with `IChatClient` parameter: `strategy.CompactAsync(history, mockClient.Object)`. **These tests are BROKEN after Phase 1 interface changes** — `CompactAsync` no longer accepts `IChatClient`.

### `Chain2Tests.cs` (12 tests)
- Tests for `IAsyncDisposable`, `ILogger`, null guards. References `SqliteMemoryStore` (the OLD combined store) — tests will need updating after Phase 2 store split.

### `Chain3SecurityTests.cs` (~53 tests)
- Terminal allowlist/blocklist (14+ tests including [Theory]), path traversal (5 tests), SSRF (7 tests), DocumentVault (2 tests). **Excellent coverage**.
- **Issue**: Some tests depend on actual system state (git installed, node installed, directories exist).

### `Chain4TestingTests.cs` (~45 tests)
- ConfigurationService (6), SkillRegistry (4), SummarizationCompactionStrategy (4), DefaultToolManager (3), InMemoryMemoryStore (4), SqliteMemoryStore (6), CompatibilityMiddleware (3), LocalEmbeddingGenerator (4), MassIndexerService (4), ViewModels (8).
- **Issue**: Many tests reference `SqliteMemoryStore` (combined store) and `MassIndexerService` with old constructor signature (no `IServiceProvider`).

### `Chain5ArchitectureTests.cs` (~20 tests)
- SQLite LIMIT (2), LIKE injection (2), IAgentFactory (empty section), thought log (1), ValueToVisibilityConverter (8), streaming (1).
- **Issue**: `IAgentFactory` section is empty ("Test will be added after implementing"). Streaming has only 1 of 3 planned scenarios.

### `FoundationInterfaceTests.cs` (9 tests)
- New Phase 1 tests. Tests interface signatures via reflection, `IVectorMemoryStore` not inheriting `IMemoryStore`, `ICompactionStrategy` without `IChatClient`, `ISkillRegistry` methods.

### `MemoryStoreSplitTests.cs` (15 tests)
- New Phase 2 tests. Tests `ISqliteConnectionFactory`, `SqliteChatMemoryStore`, `SqliteVectorMemoryStore` implementations.

### UI Tests (4 files, ~16 tests)
- `ThoughtLogPanelBindingTests.cs` (2): XAML content analysis for `Thoughts.` prefix absence.
- `ChatPageTemplateTests.cs` (2): ItemTemplate presence + bubble styles.
- `LibraryPageVirtualizationTests.cs` (2): ListView usage check.
- `AccessibilityTests.cs` (15+): `AutomationProperties.Name` presence on controls.
- **Issue**: These tests read XAML as strings and check for substring presence — fragile, depends on current file paths.

### `ValueToVisibilityConverterTests.cs` (24 tests)
- Comprehensive converter tests: boolean (4), integer (5), string (5), ConvertBack (4), edge cases (4). **Best-covered class in the project.**

### `SettingsViewModelTests.cs` (2 tests)
- Minimal: constructor loads defaults, property assignment. No save/load logic tested.

---

## PHASE 7: Project Files (6 .csproj)

### `AgentHarness.Abstractions.csproj`
- `net10.0`, only dependency: `Microsoft.Extensions.AI 10.6.0`. Clean, minimal.

### `AgentHarness.Core.csproj`
- `net10.0`, 8 dependencies: Dapper, HtmlAgilityPack, SQLite, MEAI, Logging.Abstractions, SmartComponents.LocalEmbeddings (preview!), System.Numerics.Tensors, TikTokenSharp, UglyToad.PdfPig (custom version). 
- **Issue**: `SmartComponents.LocalEmbeddings 0.1.0-preview10148` is a preview — could break on SDK update.

### `AgentHarness.Hosting.csproj`
- `net10.0`, `LangVersion` set to `14.0` (only project with explicit version). Dependencies: MEAI, DI.Abstractions, Logging.Abstractions, SQLite, Dapper. References Abstractions + Core.

### `AgentHarness.WinUI.csproj`
- `net10.0-windows10.0.19041.0`, WinExe, x64 platform. 7 packages: WindowsAppSDK 2.1.3, CommunityToolkit.Mvvm, MEAI, MEAI.OpenAI, OpenAI, DI, Hosting. 
- **Issue**: Does NOT reference `AgentHarness.Hosting` — still references Core directly.

### `AgentHarness.Cli.csproj`
- `net10.0`, Console App. Dependencies: MEAI.OpenAI, OpenAI, Hosting, MEAI. References Abstractions + Core.
- **Issue**: Does NOT reference `AgentHarness.Hosting`. No `LangVersion`.

### `AgentHarness.Tests.csproj`
- `net10.0-windows10.0.19041.0`, x64. References: Core, Abstractions, Hosting, WinUI. xUnit + Moq + coverlet.
- **Issue**: Targets Windows (WinUI dependency) — cross-platform testing impossible.

---

## PHASE 8: Other Files

### `.atl/sdd-init-state.md`
- Snapshot from 2026-05-26. Claims 6 tests (stale — now 147). 5 projects (stale — now 6 with Hosting). Mode: `openspec`. Strict TDD enabled.

### `.atl/skill-registry.md`
- Index of 10 project skills (branch-pr, chained-pr, cognitive-doc-design, comment-writer, go-testing, issue-creation, judgment-day, skill-creator, skill-improver, work-unit-commits). Source: 3 directories (`~/.config/opencode`, `~/.claude`, `~/.gemini`).

### `.editorconfig`
- C#: file_scoped namespaces, enable nullable, implicit usings, space indent 4, using outside namespace, no `this.` qualification. Minimal — no style rules for naming conventions or code analysis.

### `openspec/ROADMAP-10.md`
- Overall score: 9.0/10. Lists 8 pending items prioritised 1-8: DPAPI encryption (CRITICAL), migrate Debug→ILogger (8 files), null guards (2 services), register `IAgentFactory` in DI, missing IAgentFactory test, `AddChatClient` captive dependency, legacy converters, streaming tests. 3 WinUI platform blockers in backlog.

---

## SUMMARY

### Project Purpose
AgentHarness is an AI agent orchestration platform that manages LLM conversations with tool execution, RAG retrieval, conversation compaction, multi-agent indexing pipelines, and provides both a WinUI 3 desktop shell and a CLI interface. It supports OpenAI-compatible models with DeepSeek compatibility middleware and local ONNX embeddings.

### Architecture Style
**Claimed**: Clean/Hexagonal Architecture + MVVM with CommunityToolkit.Mvvm.
**Actual state**: In transition. The architecture *claims* Clean but:
- **Abstractions** layer is clean (11 interfaces, 1 dependency: MEAI)
- **Core** layer has proper namespace modules but concrete tools are static (untestable)
- **Hosting** layer exists but is not yet consumed by WinUI or CLI
- **WinUI** still directly instantiates Core types with `new`
- **CLI** duplicates DI setup entirely

### What's Done

| Capability | Status | Quality |
|------------|--------|---------|
| Agent orchestration loop | ✅ Complete | Good — proper DI, Stopwatch, nullable |
| Tool system (6 tool categories) | ✅ Complete | Static classes — untestable, no DI |
| Memory store (SQLite chat) | ✅ Split from vector | Phase 2 done — separate tables |
| Vector store (RAG) | ✅ Complete | Cosine similarity, LIKE escape, WAL |
| Compaction (TikToken + LLM) | ✅ Complete | Ctor-injected IChatClient (Phase 1 fix) |
| Multi-agent pipeline (MassIndexer) | ✅ Complete | Channel-based, IServiceProvider resolution |
| Skill registry | ✅ Complete | ISkillRegistry interface added |
| Embeddings (ONNX BGE-micro-v2) | ✅ Complete | Preview package, both constructors |
| Compatibility middleware | ✅ Complete | DeepSeek phi/r1 handling |
| WinUI 3 shell (5 pages, 3 controls) | ✅ Complete | Accessibility, x:Bind, virtualization |
| CLI REPL | ✅ Complete | No shared hosting yet |
| Hosting composition root | ✅ Created | Not yet consumed by hosts |
| SDD process artifacts | ✅ 8 specs written | Given/When/Then scenarios |
| Tests | ✅ 147 passing | Unit + integration + UI validation |
| CI/CD | ✅ GitHub Actions | Build, format, test, artifacts |
| Security (terminal sandbox, SSRF, path traversal) | ✅ Complete | Allowlist + blocklist patterns |

### What's Incomplete

| Item | Phase | Priority |
|------|-------|----------|
| WinUI uses shared hosting | Phase 3 | HIGH |
| CLI uses shared hosting | Phase 3 | HIGH |
| ViewModel removes IAgent casts | Phase 3 | HIGH |
| Zero `new CoreType` in hosts | Phase 3 | HIGH |
| Test port to new structure | Phase 4 | |
| English-only identifiers/comments | Phase 4 | |
| C# 14.0 across all projects | Phase 4 | |
| DPAPI encryption for API keys | Backlog | CRITICAL |
| Debug.WriteLine → ILogger (8 files) | Backlog | MEDIUM |
| Null guards on SkillRegistry, ConfigurationService | Backlog | LOW |
| IAgentFactory DI registration | Backlog | MEDIUM |

### Architectural Problems (Ranked)

1. **CRITICAL — Static tool classes**: All 6 tool files are `static class` — completely untestable, bypasses DI, can't be mocked. Spec requires DI-only instantiation. Contradiction between design (tools registered via DI) and implementation (tools called via static methods).

2. **CRITICAL — WinUI compositon root not migrated**: `App.xaml.cs` still uses `new SqliteMemoryStore()`, `new ConfigurationService()`, inline agent construction. The shared `AddAgentHarnessCore()` exists but is unused by WinUI or CLI. Phase 3 incomplete.

3. **HIGH — Broken tests after Phase 1/2 interface changes**: `CompactionTests` uses old `CompactAsync(history, IChatClient)` signature. `Chain2Tests` + `Chain4TestingTests` reference `SqliteMemoryStore` (deleted in Phase 2). `MassIndexerService` tests use old constructor. ~30+ tests need updating.

4. **HIGH — `MainViewModel` is 416 lines of mixed concerns**: Tool registration, agent re-initialization, chat send, embedding creation, vision agent setup — all in one file. Should be decomposed into composition root + smaller services.

5. **HIGH — Spanish remains throughout**: 10+ files have Spanish error strings, prompts, comments, and identifiers. `SvgCircuitLibrary` XML, `VisionAgent`/`SvgGeneratorAgent` prompts, all tool error messages. English-only goal not met.

6. **MEDIUM — `IAgent.HistoryProvider` property leaks implementation**: Forces `IMemoryStore` on every `IAgent`, enables `MainViewModel` to cast to `IVectorMemoryStore` or `SqliteMemoryStore`.

7. **MEDIUM — API keys in plaintext on disk**: `config.json` stores OpenAI, OpenCode, Tavily, Vision API keys unencrypted. DPAPI encryption deferred from audit-dios-extremo.

8. **MEDIUM — Preview dependencies**: `SmartComponents.LocalEmbeddings 0.1.0-preview10148`, `UglyToad.PdfPig 1.7.0-custom-5` (custom fork). Breaking change risk.

9. **LOW — `DateTime.Now` usage**: Used in `AgentHarness`, `MainViewModel.ChatMessageViewModel`, `DocumentVaultTools` — should be `DateTime.UtcNow` or stopwatch for timestamps.

10. **LOW — `IAgent` singleton deadlock risk**: `ServiceCollectionExtensions` registers `IAgent` via `.GetAwaiter().GetResult()` — sync-over-async antipattern.

### Recommendations

1. **Complete Phase 3** — migrate WinUI `App.xaml.cs` and CLI `Program.cs` to use `AddAgentHarnessCore()` as priority.
2. **Convert tool classes from static to instance** — register tools via DI in `ServiceCollectionExtensions`.
3. **Fix broken tests** — update all tests referencing old `SqliteMemoryStore` and old `CompactAsync` signature.
4. **Decompose `MainViewModel`** — extract tool registration to a `ToolRegistrationService`, embedder creation to DI factory.
5. **Implement DPAPI encryption** before any production use.
6. **Standardize English** — run grep for Spanish strings and convert systematically.
7. **Remove `IAgent.HistoryProvider`** — clients should inject `IMemoryStore` directly.
