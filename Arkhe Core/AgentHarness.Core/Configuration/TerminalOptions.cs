namespace AgentHarness.Core.Configuration;

/// <summary>
/// Configuration options for terminal/sandbox execution.
/// </summary>
public class TerminalOptions
{
    /// <summary>
    /// Gets or sets whether sandbox mode is enabled. Default is true.
    /// </summary>
    public bool EnableSandbox { get; set; } = true;

    /// <summary>
    /// Gets or sets the command timeout in seconds. Default is 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum output bytes allowed. Default is 1MB.
    /// </summary>
    public int MaxOutputBytes { get; set; } = 1024 * 1024;

    /// <summary>
    /// Gets or sets the list of allowed commands.
    /// </summary>
    public List<string> AllowedCommands { get; set; } = new()
    {
        "dir", "echo", "type", "findstr", "where",
        "dotnet", "node", "python", "git", "gh", "code",
        "msbuild", "npm", "npx", "make", "cmake"
    };
}
