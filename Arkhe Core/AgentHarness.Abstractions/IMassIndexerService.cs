using System;
using System.Threading.Tasks;

namespace AgentHarness.Abstractions;

public interface IMassIndexerService
{
    event Action<AgentTechnicalEvent>? TechnicalEventEmitted;
    Task IndexDirectoryAsync(string path);
    Task<string> IndexFolderAsync(string folderPath, string tag = "Code");
}
