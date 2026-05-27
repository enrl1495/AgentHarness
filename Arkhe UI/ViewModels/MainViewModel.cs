using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Main window view model managing navigation state and footer selection.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private PageKey _selectedPage = PageKey.Home;

    [ObservableProperty]
    private string? _selectedFooterItem;

    /// <summary>
    /// Footer items for the PaneFooter SelectorBar.
    /// </summary>
    public string[] FooterItems { get; } = ["Recent Session 1", "Recent Session 2", "Recent Session 3"];
}
