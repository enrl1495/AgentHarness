namespace AgentHarness.Abstractions;

public interface IDialogService
{
    Task ShowMessageAsync(string message, string title);
    Task<bool> ShowConfirmationAsync(string message, string title);
}
