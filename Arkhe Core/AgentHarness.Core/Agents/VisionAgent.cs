using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace AgentHarness.Core.Agents;

/// <summary>
/// Sub-agent responsible for analyzing images (e.g., schematics) and returning a textual description.
/// </summary>
public class VisionAgent
{
    private readonly IChatClient _chatClient;

    public VisionAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> AnalyzeImageAsync(byte[] imageBytes, string mediaType = "image/png", CancellationToken cancellationToken = default)
    {
        // En MEAI, un ChatMessage puede contener una colección de AIContent (texto, imágenes, etc.)
        var contents = new List<AIContent>
        {
            new TextContent(@"Sos un ingeniero en electrónica experto. Analizá esta imagen en detalle.
Si es un circuito, DEBÉS devolver tu análisis en dos partes:
1. Una descripción técnica detallada en texto explicando qué hace y cómo funciona.
2. Un bloque JSON estricto al final (envuelto en ```json) que represente la topología (Netlist), con esta estructura exacta:
{
  ""name"": ""Nombre del circuito"",
  ""nodes"": [""N1_IN"", ""N2_OUT"", ""GND""],
  ""components"": [
     { ""type"": ""Resistor"", ""id"": ""R1"", ""value"": ""10k"", ""connected_to"": [""N1_IN"", ""N2_OUT""] }
  ]
}
Si la imagen no es un circuito, solo transcribí o describí el contenido normalmente."),
            new DataContent(imageBytes, mediaType)
        };

        var message = new ChatMessage(ChatRole.User, contents);

        try
        {
            var response = await _chatClient.CompleteAsync(new[] { message }, options: null, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Message.Text?.Trim() ?? "No se pudo extraer información de la imagen.";
        }
        catch (Exception ex)
        {
            return $"[Error en VisionAgent]: {ex.Message}";
        }
    }
}
