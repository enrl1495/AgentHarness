using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AgentHarness.Core.Tools;

/// <summary>
/// A tool for performing web searches. This acts as the "Context7" global knowledge provider.
/// </summary>
public class WebSearchTools
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient = new();

    public WebSearchTools(string apiKey, string endpoint)
    {
        _apiKey = apiKey;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Performs a web search to find up-to-date information on a topic.
    /// </summary>
    public async Task<string> WebSearch(string query)
    {
        if (string.IsNullOrEmpty(_apiKey)) return "Error: No se ha configurado una API Key para búsqueda web.";

        try
        {
            // Ejemplo para Tavily (especializado en agentes)
            // Si el usuario usa otro, tendríamos que ajustar el JSON
            var payload = new
            {
                query = query,
                search_depth = "basic",
                max_results = 5
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                var resultsSummary = $"Resultados de búsqueda para '{query}':\n\n";

                if (data.TryGetProperty("results", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        var title = item.GetProperty("title").GetString();
                        var content = item.GetProperty("content").GetString();
                        var url = item.GetProperty("url").GetString();
                        resultsSummary += $"* {title}\n  Contenido: {content}\n  Fuente: {url}\n\n";
                    }
                }

                return resultsSummary;
            }

            return $"Error en la búsqueda: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            return $"Error técnico al buscar en la web: {ex.Message}";
        }
    }
}
