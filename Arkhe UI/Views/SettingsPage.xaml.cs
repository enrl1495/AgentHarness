using Microsoft.UI.Xaml.Controls;
using AgentHarness.WinUI.ViewModels;

namespace AgentHarness.WinUI;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
