using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using AgentHarness.Core.Tools;

namespace AgentHarness.Core.Agents;

/// <summary>
/// Sub-agent responsible for drawing an SVG schematic from retrieved netlist JSON context.
/// </summary>
public class SvgGeneratorAgent
{
    private readonly IChatClient _chatClient;

    public SvgGeneratorAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GenerateSvgAsync(string request, string ragContext, CancellationToken cancellationToken = default)
    {
        var prompt = $@"Sos un generador experto de esquemas SVG para electrónica. 
El usuario solicita dibujar un circuito: '{request}'.

Aquí está la información recuperada de nuestra biblioteca (RAG) que contiene la topología (JSON) y descripción teórica:
--- INICIO CONTEXTO ---
{ragContext}
--- FIN CONTEXTO ---

Debés generar el código de un diagrama SVG válido y limpio basándote estrictamente en el JSON topológico proporcionado en el contexto.

Reglas INQUEBRANTABLES:
1. Usa <svg viewBox=""0 0 800 600"" xmlns=""http://www.w3.org/2000/svg"">. Fondo blanco si querés (<rect width=""100%"" height=""100%"" fill=""white""/>).
2. INCLUYE ESTE BLOQUE <defs> EXACTO AL PRINCIPIO:
{SvgCircuitLibrary.Defs}
3. Dibuja los componentes instanciando los IDs del defs usando <use href=""#id"" x=""..."" y=""..."" />.
   IDs disponibles: #resistor, #capacitor, #diode, #inductor, #gnd, #dc_source.
4. Organiza lógicamente en el espacio (entradas a la izquierda, salidas a la derecha, GND abajo). Es vital calcular bien X e Y.
5. Dibuja los cables de conexión usando <path d=""M x1 y1 L x2 y2"" stroke=""black"" fill=""none"" stroke-width=""2""/> para conectar los pines.
6. Devuelve SOLO código SVG puro. Nada de markdown, nada de comillas de código ```svg, nada de texto explicativo. Si devolvés algo que no empieza con <svg, el parser va a explotar.";

        try
        {
            var response = await _chatClient.CompleteAsync(prompt, options: null, cancellationToken: cancellationToken).ConfigureAwait(false);
            var svg = response.Message.Text?.Trim() ?? string.Empty;

            // Clean markdown if the model hallucinates it despite the prompt
            if (svg.StartsWith("```xml")) svg = svg.Substring(6);
            if (svg.StartsWith("```svg")) svg = svg.Substring(6);
            if (svg.StartsWith("```")) svg = svg.Substring(3);
            if (svg.EndsWith("```")) svg = svg.Substring(0, svg.Length - 3);

            return svg.Trim();
        }
        catch (Exception ex)
        {
            return $"<svg><text x='10' y='20'>Error generando SVG: {ex.Message}</text></svg>";
        }
    }
}
