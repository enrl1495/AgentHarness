using System.IO;

namespace AgentHarness.Core.Tools;

/// <summary>
/// A collection of tools for interacting with the local file system.
/// This acts as a mini-"Context7" to give the agent awareness of local files.
/// </summary>
public static class FileSystemTools
{
    // Allowed base directories for sandbox
    private static readonly string[] _allowedBaseDirectories = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AgentHarness", "workspace"),
        Path.GetTempPath(),
        AppDomain.CurrentDomain.BaseDirectory
    };

    /// <summary>
    /// Validates that a path is within allowed sandbox directories.
    /// </summary>
    /// <param name="path">The path to validate</param>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    private static (bool IsValid, string? ErrorMessage) ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (false, "Error: La ruta no puede estar vacía o ser nula.");
        }

        // Block UNC paths
        if (path.StartsWith(@"\\") || path.StartsWith("//"))
        {
            return (false, "Error: Las rutas UNC no están permitidas por seguridad.");
        }

        try
        {
            // Resolve to absolute path
            var fullPath = Path.GetFullPath(path);

            // Check for parent directory traversal attempts in the original path
            if (path.Contains(".."))
            {
                // Additional check: ensure resolved path doesn't escape allowed directories
                foreach (var baseDir in _allowedBaseDirectories)
                {
                    if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                    {
                        return (true, null);
                    }
                }

                // If path contains .. and doesn't resolve to allowed directory, block it
                if (!IsPathWithinAllowedDirectories(fullPath))
                {
                    return (false, "Error: Acceso denegado - intento de traversía de directorios detectado.");
                }
            }

            // Verify the resolved path is within allowed directories
            if (!IsPathWithinAllowedDirectories(fullPath))
            {
                return (false, "Error: Acceso denegado - la ruta está fuera del sandbox permitido.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: Ruta inválida - {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a path is within any of the allowed base directories.
    /// </summary>
    private static bool IsPathWithinAllowedDirectories(string fullPath)
    {
        foreach (var baseDir in _allowedBaseDirectories)
        {
            if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Lists all files and directories in a given path.
    /// </summary>
    public static string ListDirectory(string path)
    {
        // SECURITY: Validate path
        var validation = ValidatePath(path);
        if (!validation.IsValid)
        {
            return validation.ErrorMessage!;
        }

        try
        {
            if (!Directory.Exists(path)) return $"Error: El directorio '{path}' no existe.";

            var entries = Directory.GetFileSystemEntries(path);
            var result = $"Contenido de '{path}':\n" + string.Join("\n", entries.Select(e => Path.GetFileName(e)));
            return result;
        }
        catch (Exception ex)
        {
            return $"Error al listar directorio: {ex.Message}";
        }
    }

    /// <summary>
    /// Reads the content of a specific file.
    /// </summary>
    public static string ReadFile(string path)
    {
        // SECURITY: Validate path
        var validation = ValidatePath(path);
        if (!validation.IsValid)
        {
            return validation.ErrorMessage!;
        }

        try
        {
            if (!File.Exists(path)) return $"Error: El archivo '{path}' no existe.";

            // Limitamos la lectura por seguridad y contexto
            var content = File.ReadAllText(path);
            if (content.Length > 10000)
            {
                return content.Substring(0, 10000) + "\n... [ARCHIVO TRUNCADO POR TAMAÑO]";
            }
            return content;
        }
        catch (Exception ex)
        {
            return $"Error al leer archivo: {ex.Message}";
        }
    }
}
