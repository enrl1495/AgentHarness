# Arkhe UI

Capa de presentación y entry points del harness. Acá van el shell WinUI 3 desktop y el CLI REPL headless (y cualquier host futuro).

## Trazabilidad

- **Core / Motor**: la lógica de negocio, orquestador, tools y stores viven en [`../Arkhe Core/`](../Arkhe%20Core/). Los hosts consumen el composition root `AddAgentHarnessCore()` del proyecto `AgentHarness.Hosting`.
- **Proyecto integrado**: el ensamble completo Core+UI está en [`../Arkhe/`](../Arkhe/) — la aplicación final.

**Regla de arquitectura**: cero `new` sobre tipos concretos del Core. Todo por DI desde `AddAgentHarnessCore()`.

## Contexto histórico

Si no conocés tu propia historia, estás condenado a repetirla. Los documentos de la raíz cuentan de dónde viene este proyecto y por qué falló su predecesor:

- [`../arquitectura-subagente.md`](../arquitectura-subagente.md) — reporte exhaustivo de arquitectura.
- [`../resumen-orquestador.md`](../resumen-orquestador.md) — resumen ejecutivo: evolución, problemas, estado.
