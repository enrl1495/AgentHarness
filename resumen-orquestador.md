# AgentHarness — Resumen del Orquestador

## Qué es
Un **harness de orquestración de agentes AI en .NET 10** — maneja el ciclo de vida de agentes LLM, ejecución de herramientas, memoria/RAG, compactación de contexto, pipelines multi-agente, y tiene un shell WinUI 3 + CLI.

## La evolución (orden → desorden → evolución)

### 1. Build inicial
1 commit inicial — un harness funcional con WinUI shell, 6 herramientas, memoria SQLite única (chat+vectores juntos), ~5 tests.

### 2. `audit-dios-extremo` (archivado)
Auditoría de seguridad del backend. 25 findings (5 CRITICAL). Entregó: sandbox de terminal con allowlist, protección SSRF, path traversal, `IAsyncDisposable`, `IAgentFactory`, tests de 5→117. Score: 6.7→8.5.

### 3. `audit-winui-layer` (archivado)
Auditoría de accesibilidad/XAML. 22 issues (3 CRITICAL). Entregó: `AutomationProperties.Name` en 42 controles, `x:Bind` en ~70%, virtualización en LibraryPage, colores hardcodeados→theme resources. 30 tests nuevos.

### 4. `rebuild-v2` (en progreso, 15/28 tareas)
Refactor arquitectónico porque el código decía "Clean Architecture" pero tenía acoplamientos concretos por todos lados:

- **Fase 1 ✅** (6/6): `ICompactionStrategy` desacoplado de `IChatClient`, `IVectorMemoryStore` separado de `IMemoryStore`, `ISkillRegistry` como interfaz, `IAgentFactory`, Foundation tests.
- **Fase 2 ✅** (9/9): Stores SQLite spliteados (chat y vectores independientes), `ISqliteConnectionFactory` compartido, proyecto `AgentHarness.Hosting` con composition root `AddAgentHarnessCore()`.
- **Fase 3 ❌** (pendiente): Migrar WinUI `App.xaml.cs` y CLI `Program.cs` a usar el Hosting compartido. Sacar los `new ConcreteType()` de los hosts.
- **Fase 4 ❌** (pendiente): Portar tests, inglés en todo el código, C# 14.0 unificado.

---

## Los 10 problemas principales

### CRITICAL
1. **Las 6 herramientas son `static class`**: `TerminalTools`, `WebScraperTools`, `FileSystemTools`, etc. Intesteables, sin DI. La spec dice "DI-only instantiation".
2. **WinUI y CLI no usan el Hosting nuevo**: `App.xaml.cs` sigue haciendo `new SqliteMemoryStore()`, `new ConfigurationService()` a mano. `AddAgentHarnessCore()` existe pero nadie lo llama.

### HIGH
3. **~30 tests rotos**: `CompactionTests` usa firma vieja de `CompactAsync`, `Chain2Tests`/`Chain4TestingTests` referencian `SqliteMemoryStore` que ya no existe.
4. **`MainViewModel` tiene 416 líneas**: Tool registration, re-inicialización de agentes, embeddings, vision — todo en un archivo.
5. **Español por todos lados**: 10+ archivos con errores, prompts y comentarios en español. `VisionAgent`, `SvgGeneratorAgent`, todas las tools, `SkillRegistry`.

### MEDIUM
6. **`IAgent.HistoryProvider` expone `IMemoryStore`**: Permite casts a tipos concretos desde los ViewModels.
7. **API keys en plaintext**: `config.json` sin encryptar. DPAPI encryption diferido del audit-dios-extremo.
8. **Dependencias preview**: `SmartComponents.LocalEmbeddings 0.1.0-preview10148`.

### LOW
9. **`DateTime.Now` en vez de `UtcNow`**: En `AgentHarness`, `MainViewModel`, `DocumentVaultTools`.
10. **Deadlock risk**: `IAgent` singleton registrado con `.GetAwaiter().GetResult()`.

---

## Estado general

| Capa | Estado |
|------|--------|
| Abstractions (11 interfaces) | ✅ Clean |
| Core (implementación) | ⚠️ Tools estáticas, español |
| Hosting (composition root) | ✅ Creado pero sin consumir |
| WinUI | ❌ Sin migrar al hosting |
| CLI | ❌ Sin migrar al hosting |
| Tests (147) | ⚠️ ~30 rotos por el refactor |

El proyecto está en un punto de transición: la base arquitectónica nueva (Abstractions + Core + Hosting) está sólida, pero los hosts (WinUI, CLI) todavía viven en el pasado. Las piezas están listas, falta conectarlas.
