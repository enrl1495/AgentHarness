namespace AgentHarness.Abstractions;

public interface IConfigurationService
{
    string GetSystemPrompt();
    T GetSetting<T>(string key);
}
