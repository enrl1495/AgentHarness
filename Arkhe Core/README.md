# Arkhe Core

Motor de orquestración de agentes AI. Contiene las abstracciones (11 interfaces), la implementación del orquestador, el pipeline multi-agente, memoria SQLite (chat + vectores), compactación de contexto, tools, y el composition root de DI.

**Documentación interna**: [ARCHITECTURE.md](./ARCHITECTURE.md)

## Proyectos

| Proyecto | Rol |
|----------|-----|
| `AgentHarness.Abstractions` | Interfaces puras — sin dependencias de UI ni infraestructura |
| `AgentHarness.Core` | Implementación concreta — orquestador, tools, stores, servicios |
| `AgentHarness.Hosting` | Composition root DI — `AddAgentHarnessCore()` |

## Trazabilidad

- **UI / Hosts**: la contraparte visual y CLI vive en [`../Arkhe UI/`](../Arkhe%20UI/) — WinUI 3 desktop shell y CLI REPL.
- **Proyecto integrado**: el ensamble completo Core+UI está en [`../Arkhe/`](../Arkhe/) — la aplicación final que une ambas partes.

## Contexto histórico

Si no conocés tu propia historia, estás condenado a repetirla. Los documentos de la raíz cuentan de dónde viene este proyecto, por qué falló su predecesor, y qué problemas se encontraron en el camino:

- [`../arquitectura-subagente.md`](../arquitectura-subagente.md) — reporte exhaustivo de arquitectura (442 líneas): cada interfaz, cada clase, cada decisión y cada problema.
- [`../resumen-orquestador.md`](../resumen-orquestador.md) — resumen ejecutivo: qué es, cómo evolucionó, los 10 problemas principales, y el estado actual.
