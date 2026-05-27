# Arkhe

Proyecto integrado — ensambla el Core y la UI en una aplicación funcional completa. Este es el punto de entrada final: acá se juntan el motor de orquestración y los hosts (WinUI + CLI).

## Trazabilidad

- **Core / Motor**: la lógica de orquestración, abstracciones, tools y stores → [`../Arkhe Core/`](../Arkhe%20Core/)
- **UI / Hosts**: el shell WinUI 3 y el CLI REPL → [`../Arkhe UI/`](../Arkhe%20UI/)

Cada uno tiene su propio README con documentación interna y referencias cruzadas.

## Contexto histórico

Si no conocés tu propia historia, estás condenado a repetirla. Antes de tocar nada, leé los documentos de la raíz:

- [`../arquitectura-subagente.md`](../arquitectura-subagente.md) — reporte exhaustivo de arquitectura: cada decisión, cada interfaz, cada problema encontrado.
- [`../resumen-orquestador.md`](../resumen-orquestador.md) — resumen ejecutivo: build inicial → auditorías → rebuild-v2, los 10 problemas principales, estado por capa.
