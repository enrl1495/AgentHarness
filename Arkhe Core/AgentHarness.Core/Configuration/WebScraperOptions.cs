namespace AgentHarness.Core.Configuration;

/// <summary>
/// Configuration options for web scraper.
/// </summary>
public class WebScraperOptions
{
    /// <summary>
    /// Gets or sets the request timeout in seconds. Default is 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum response bytes allowed. Default is 1MB.
    /// </summary>
    public int MaxResponseBytes { get; set; } = 1024 * 1024;

    /// <summary>
    /// Gets or sets the list of allowed URL schemes. Default is http and https only.
    /// </summary>
    public List<string> AllowedSchemes { get; set; } = new() { "http", "https" };

    /// <summary>
    /// Gets or sets the list of allowed domains (empty means all non-blocked domains are allowed).
    /// </summary>
    public List<string> AllowedDomains { get; set; } = new();

    /// <summary>
    /// Gets or sets whether private IP addresses should be blocked. Default is true.
    /// </summary>
    public bool BlockPrivateIPs { get; set; } = true;
}
