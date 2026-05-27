using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using AgentHarness.Hosting;
using AgentHarness.WinUI.ViewModels;

namespace AgentHarness.WinUI;

public partial class App : Application
{
    private IServiceProvider? _services;

    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not initialized.");

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var services = new ServiceCollection();
        
        services.AddSingleton<IChatClient, NoOpChatClient>();
        services.AddAgentHarnessCore();
        
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<SkillsViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        services.AddTransient<HomePage>();
        services.AddTransient<ChatPage>();
        services.AddTransient<LibraryPage>();
        services.AddTransient<SkillsPage>();
        services.AddTransient<SettingsPage>();
        
        _services = services.BuildServiceProvider();

        var mainWindow = new MainWindow();
        mainWindow.Activate();
    }

    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    private sealed class NoOpChatClient : IChatClient
    {
        public Task<ChatCompletion> CompleteAsync(
            IList<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "No IChatClient configured.")));
        }

        public IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
            IList<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return GetStreamingAsync();

            async IAsyncEnumerable<StreamingChatCompletionUpdate> GetStreamingAsync(
                [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                yield return new StreamingChatCompletionUpdate
                {
                    Text = "No IChatClient configured."
                };
            }
        }

        public ChatClientMetadata Metadata => new("NoOpChatClient");

        public void Dispose() { }

        public object? GetService(Type serviceType, object? key = null) => null;
    }
}
