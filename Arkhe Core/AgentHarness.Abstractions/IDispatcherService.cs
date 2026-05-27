namespace AgentHarness.Abstractions;

/// <summary>
/// Provides an abstraction for dispatching actions to the UI thread.
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    /// Executes the specified action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void RunOnUIThread(Action action);
}
