using System.Text.Json;
using AgentHarness.Abstractions;

namespace AgentHarness.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    private Dictionary<string, string> _settings = new();

    public ConfigurationService()
    {
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(ConfigurationService)}.{nameof(LoadConfiguration)}] {ex.GetType().Name}: {ex.Message}");
                /* Log error if necessary */
            }
        }
    }

    public string GetValue(string key, string defaultValue = "")
    {
        if (_settings.TryGetValue(key, out var value)) return value;
        return Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }

    public string GetSystemPrompt() => GetValue("SystemPrompt", "You are a helpful assistant.");

    public T GetSetting<T>(string key)
    {
        var value = GetValue(key);
        return (T)Convert.ChangeType(value, typeof(T));
    }
}
