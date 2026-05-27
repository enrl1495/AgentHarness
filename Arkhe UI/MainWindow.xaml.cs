using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using AgentHarness.WinUI.ViewModels;

namespace AgentHarness.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;

    private static readonly Dictionary<string, Type> PageMap = new()
    {
        ["Home"] = typeof(HomePage),
        ["Chat"] = typeof(ChatPage),
        ["Library"] = typeof(LibraryPage),
        ["Skills"] = typeof(SkillsPage),
        ["Settings"] = typeof(SettingsPage)
    };

    public MainWindow()
    {
        InitializeComponent();
        
        // Resolve services from DI
        var app = Application.Current as App;
        _services = app?.Services ?? throw new InvalidOperationException("App.Services not available.");
        _viewModel = _services.GetRequiredService<MainViewModel>();
        
        // Set initial page to Home using DI-resolved instance
        var homePage = _services.GetRequiredService<HomePage>();
        ContentFrame.Content = homePage;
        
        // Set initial selection
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            if (tag != null && PageMap.TryGetValue(tag, out var pageType))
            {
                // Resolve page from DI with its ViewModel injected
                var page = _services.GetRequiredService(pageType) as Page;
                ContentFrame.Content = page;
            }
        }
    }
}
