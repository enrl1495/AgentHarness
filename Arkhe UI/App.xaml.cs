using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
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
        // Build service provider with Core services
        var services = new ServiceCollection();
        
        // Call AddAgentHarnessCore() to register all Core services
        // Note: IChatClient and IEmbeddingGenerator are NOT registered by AddAgentHarnessCore()
        // They should be registered separately by the host when Settings configures an endpoint
        services.AddAgentHarnessCore();
        
        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<SkillsViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        // Register Pages for DI navigation
        services.AddTransient<HomePage>();
        services.AddTransient<ChatPage>();
        services.AddTransient<LibraryPage>();
        services.AddTransient<SkillsPage>();
        services.AddTransient<SettingsPage>();
        
        _services = services.BuildServiceProvider();

        // Create and activate main window
        var mainWindow = new MainWindow();
        mainWindow.Activate();
    }

    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }
}
