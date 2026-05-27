using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Security;

namespace AgentHarness.Core.Tools;

/// <summary>
/// Tools for scraping and cleaning web content for the agent to study.
/// This acts as a "mini-Context7" for personal documentation indexing.
/// </summary>
public static class WebScraperTools
{
    private static readonly HttpClient _httpClient = new();

    // Blocked IP ranges (RFC 1918 private, RFC 4193 unique-local, link-local, loopback)
    private static readonly Regex _blockedIpPattern = new(
        @"^(localhost|127\.\d+\.\d+\.\d+|::1|0\.0\.0\.0|" +
        @"10\.\d+\.\d+\.\d+|172\.(1[6-9]|2\d|3[01])\.\d+\.\d+|" +
        @"192\.168\.\d+\.\d+|169\.254\.\d+\.\d+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Validates a URL for SSRF prevention.
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    private static (bool IsValid, string? ErrorMessage) ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return (false, "Error: La URL no puede estar vacía.");
        }

        // Parse URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return (false, "Error: URL inválida - debe ser una URL absoluta completa.");
        }

        // SECURITY CHECK 1: Only allow http and https schemes
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return (false, $"Error: El protocolo '{uri.Scheme}' no está permitido. Solo http:// y https:// son aceptados.");
        }

        // SECURITY CHECK 2: Block localhost and loopback
        var host = uri.Host.ToLowerInvariant();
        if (host == "localhost" || host == "127.0.0.1" || host == "::1" || host == "0.0.0.0")
        {
            return (false, $"Error: La URL '{host}' no está permitida - acceso a localhost bloqueado.");
        }

        // SECURITY CHECK 3: Block private IP ranges (RFC 1918, RFC 4193)
        if (_blockedIpPattern.IsMatch(host))
        {
            return (false, $"Error: La URL '{host}' no está permitida - acceso a IPs privadas/internal bloqueado.");
        }

        // SECURITY CHECK 4: Block cloud metadata endpoints
        if (host == "169.254.169.254" ||
            host.EndsWith(".metadata.google.internal") ||
            host.EndsWith(".internal"))
        {
            return (false, $"Error: La URL '{host}' no está permitida - acceso a endpoints de metadata bloqueado.");
        }

        return (true, null);
    }

    /// <summary>
    /// Fetches a URL and returns a cleaned, plain-text version of its content.
    /// It removes scripts, styles, and navigation elements to focus on documentation.
    /// </summary>
    public static async Task<string> FetchAndCleanUrl(string url)
    {
        // SECURITY: Validate URL before making request
        var validation = ValidateUrl(url);
        if (!validation.IsValid)
        {
            return validation.ErrorMessage!;
        }

        try
        {
            // Add request timeout (10 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var html = await _httpClient.GetStringAsync(url, cts.Token);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 1. Eliminar elementos de ruido
            var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//nav|//footer|//header|//iframe|//aside");
            if (nodesToRemove != null)
            {
                foreach (var node in nodesToRemove) node.Remove();
            }

            // 2. Extraer texto plano
            var text = doc.DocumentNode.InnerText;

            // 3. Limpiar espacios en blanco excesivos
            text = Regex.Replace(text, @"\s+", " ").Trim();

            // Decodificar entidades HTML (como &nbsp; o &quot;)
            text = System.Net.WebUtility.HtmlDecode(text);

            // Max response size (5MB)
            if (text.Length > 5 * 1024 * 1024)
            {
                return text.Substring(0, 5 * 1024 * 1024) + "\n... [CONTENIDO TRUNCADO POR TAMAÑO (límite 5MB)]";
            }

            return text;
        }
        catch (OperationCanceledException)
        {
            return "Error: La solicitud excedió el tiempo límite de 10 segundos.";
        }
        catch (Exception ex)
        {
            return $"Error al procesar la URL: {ex.Message}";
        }
    }

    /// <summary>
    /// Chunks a long text into smaller pieces for embedding.
    /// </summary>
    public static List<string> ChunkText(string text, int chunkSize = 1000)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(text)) return chunks;

        for (int i = 0; i < text.Length; i += chunkSize)
        {
            int length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }
        return chunks;
    }

    /// <summary>
    /// Chunks text semantically by trying to split on double newlines (paragraphs/methods) up to a maximum size.
    /// </summary>
    public static List<string> SemanticChunkText(string text, int maxChunkSize = 6000)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(text)) return chunks;

        var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.None);
        var currentChunk = new System.Text.StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (currentChunk.Length + paragraph.Length > maxChunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            // If a single paragraph is larger than maxChunkSize, we must brute-force split it
            if (paragraph.Length > maxChunkSize)
            {
                var bruteChunks = ChunkText(paragraph, maxChunkSize);
                chunks.AddRange(bruteChunks);
            }
            else
            {
                currentChunk.Append(paragraph).Append("\n\n");
            }
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    /// <summary>
    /// Downloads a remote file and saves it to a local path.
    /// </summary>
    public static async Task DownloadFileAsync(string url, string targetPath)
    {
        // SECURITY: Validate URL before downloading
        var validation = ValidateUrl(url);
        if (!validation.IsValid)
        {
            throw new SecurityException(validation.ErrorMessage);
        }

        var bytes = await _httpClient.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(targetPath, bytes);
    }
}
