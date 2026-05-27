using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AgentHarness.Core.Tools;

/// <summary>
/// Tools for processing and archiving local documents.
/// </summary>
public static class DocumentVaultTools
{
    private static readonly string VaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AgentHarness",
        "docs");

    static DocumentVaultTools()
    {
        if (!Directory.Exists(VaultPath)) Directory.CreateDirectory(VaultPath);
    }

    /// <summary>
    /// Validates a filename to prevent path injection attacks.
    /// </summary>
    /// <param name="filename">The filename to validate</param>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    private static (bool IsValid, string? ErrorMessage) ValidateFilename(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return (false, "Error: El nombre del archivo no puede estar vacío.");
        }

        // Block path separators in filename
        if (filename.Contains(Path.DirectorySeparatorChar) ||
            filename.Contains(Path.AltDirectorySeparatorChar) ||
            filename.Contains("..") ||
            filename.Contains(":"))
        {
            return (false, "Error: El nombre del archivo no puede contener separadores de ruta o caracteres especiales.");
        }

        // Block null bytes
        if (filename.Contains('\0'))
        {
            return (false, "Error: El nombre del archivo contiene caracteres inválidos.");
        }

        return (true, null);
    }

    /// <summary>
    /// Copies a file to the internal vault and returns the new local path.
    /// </summary>
    public static string ArchiveFile(string sourcePath)
    {
        // SECURITY: Validate filename to prevent path injection BEFORE checking file existence
        var filename = Path.GetFileName(sourcePath);
        var filenameValidation = ValidateFilename(filename);
        if (!filenameValidation.IsValid)
        {
            throw new ArgumentException(filenameValidation.ErrorMessage, nameof(sourcePath));
        }

        // Also validate the full source path doesn't contain injection
        if (sourcePath.Contains("..") && !Path.IsPathRooted(sourcePath))
        {
            // Relative path with traversal - reject
            throw new ArgumentException("Error: La ruta no puede contener traversía de directorios (..).", nameof(sourcePath));
        }

        if (!File.Exists(sourcePath)) throw new FileNotFoundException("El archivo original no existe.", sourcePath);

        var fileName = Path.GetFileName(sourcePath);
        var targetPath = Path.Combine(VaultPath, fileName);

        if (File.Exists(targetPath))
        {
            var nameOnly = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            targetPath = Path.Combine(VaultPath, $"{nameOnly}_{DateTime.Now:yyyyMMddHHmmss}{ext}");
        }

        File.Copy(sourcePath, targetPath);
        return targetPath;
    }

    /// <summary>
    /// Extracts all text from a PDF file using the Python PyMuPDF4LLM microservice.
    /// This preserves tables and layout as Markdown.
    /// </summary>
    public static string ExtractTextFromPdf(string pdfPath)
    {
        try
        {
            // Ya no usamos entorno virtual hardcodeado, usamos el python del sistema
            string pythonExe = "python";

            // Localizamos el script en la raíz del proyecto
            string baseDir = AppContext.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            string scriptPath = Path.Combine(projectRoot, "pdf_extractor.py");

            if (!File.Exists(scriptPath))
            {
                return $"Error: No se encontró el script pdf_extractor.py en: {projectRoot}";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" \"{pdfPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || output.StartsWith("Error"))
            {
                return $"Error del Microservicio PDF:\n{error}\n{output}";
            }

            return output;
        }
        catch (Exception ex)
        {
            return $"Error al comunicar con el microservicio PDF: {ex.Message}";
        }
    }

    public static string GetVaultPath() => VaultPath;

    /// <summary>
    /// Extrae las imágenes de un PDF usando UglyToad.PdfPig, devolviendo el número de página y los bytes de la imagen.
    /// </summary>
    public static List<(int PageNumber, byte[] ImageBytes, string MediaType)> ExtractImagesFromPdf(string pdfPath)
    {
        var images = new List<(int, byte[], string)>();
        try
        {
            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                foreach (Page page in document.GetPages())
                {
                    foreach (IPdfImage image in page.GetImages())
                    {
                        if (image.TryGetPng(out byte[] pngBytes))
                        {
                            images.Add((page.Number, pngBytes, "image/png"));
                        }
                        else if (image.TryGetBytes(out IReadOnlyList<byte> bytes))
                        {
                            images.Add((page.Number, bytes.ToArray(), "image/jpeg"));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extrayendo imágenes de {pdfPath}: {ex.Message}");
        }
        return images;
    }
}
