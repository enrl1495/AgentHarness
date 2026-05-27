using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AgentHarness.Core.Tools;

/// <summary>
/// Tools for executing terminal commands. This gives the agent "hands" 
/// to build, test, and manage the project environment.
/// </summary>
public static class TerminalTools
{
    // Allowlist of safe commands (read-only or dev-build commands only)
    private static readonly HashSet<string> _allowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "dir", "echo", "type", "findstr", "where",
        "dotnet", "node", "python", "git", "gh",
        "code", "msbuild", "npm", "npx", "make", "cmake",
        "cargo", "rustc", "go", "javac", "java", "pwsh"
    };

    // Explicitly blocked commands (destructive or dangerous)
    private static readonly HashSet<string> _blockedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "del", "erase", "format", "rm", "rmdir",
        "Invoke-Expression", "iex", "Start-Process", "Invoke-Command",
        "wget", "curl", "powershell"
    };

    // Regex patterns for detecting dangerous argument patterns
    private static readonly Regex _dangerousPatterns = new(
        @"(\|\||&&|`|\$\(.*\)|Invoke-Expression|iex|New-Object.*WebClient|DownloadString|DownloadFile)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Executes a shell command with security validation (allowlist + argument validation).
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="workingDirectory">Working directory (defaults to app base directory)</param>
    /// <returns>Command output or error message</returns>
    public static async Task<string> RunCommand(string command, string? workingDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return "[ERROR]: El comando no puede estar vacío.";
        }

        // Extract the base command (first word)
        var commandParts = command.Trim().Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
        var baseCommand = commandParts[0];

        // SECURITY CHECK 1: Block explicitly dangerous commands
        if (_blockedCommands.Contains(baseCommand))
        {
            return $"[SEGURIDAD]: El comando '{baseCommand}' no está permitido por ser potencialmente peligroso.";
        }

        // SECURITY CHECK 2: Only allow commands on the allowlist
        if (!_allowedCommands.Contains(baseCommand))
        {
            return $"[SEGURIDAD]: El comando '{baseCommand}' no está en la lista de comandos permitidos.";
        }

        // SECURITY CHECK 3: Validate arguments for dangerous patterns
        if (commandParts.Length > 1)
        {
            var arguments = commandParts[1];
            if (_dangerousPatterns.IsMatch(arguments))
            {
                return "[SEGURIDAD]: El comando contiene patrones peligrosos en los argumentos (pipes, inyección, o ejecución remota).";
            }
        }

        try
        {
            // Convertimos el comando a Base64 (UTF-16LE es lo que espera PowerShell para -EncodedCommand)
            var commandBytes = Encoding.Unicode.GetBytes(command);
            var encodedCommand = Convert.ToBase64String(commandBytes);

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -EncodedCommand {encodedCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 30-second timeout with process kill
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                return "[TIMEOUT]: El comando fue abortado por seguridad (límite de 30 segundos).";
            }

            var result = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            if (process.ExitCode != 0)
            {
                return $"[ERROR (ExitCode: {process.ExitCode})]:\n{error}\n{result}";
            }

            // Max output size limit (10KB)
            if (result.Length > 10240)
            {
                return result.Substring(0, 10240) + "\n... [SALIDA TRUNCADA POR TAMAÑO (límite 10KB)]";
            }

            return string.IsNullOrEmpty(result) ? "Éxito (sin salida)." : result;
        }
        catch (OperationCanceledException)
        {
            return "[TIMEOUT]: El comando fue abortado por seguridad.";
        }
        catch (Exception ex)
        {
            return $"[ERROR CRÍTICO]: {ex.Message}";
        }
    }
}
