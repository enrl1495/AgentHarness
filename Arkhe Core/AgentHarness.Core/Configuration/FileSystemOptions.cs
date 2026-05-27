namespace AgentHarness.Core.Configuration;

/// <summary>
/// Configuration options for file system access.
/// </summary>
public class FileSystemOptions
{
    /// <summary>
    /// Gets or sets the list of allowed sandbox directories.
    /// Defaults to user's AgentHarness workspace folder.
    /// </summary>
    public List<string> SandboxDirectories { get; set; } = new()
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AgentHarness", "workspace")
    };

    /// <summary>
    /// Gets or sets whether UNC paths are allowed. Default is false.
    /// </summary>
    public bool AllowUncPaths { get; set; } = false;
}
