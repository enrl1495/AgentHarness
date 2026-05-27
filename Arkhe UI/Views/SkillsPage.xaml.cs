using Microsoft.UI.Xaml.Controls;
using AgentHarness.WinUI.ViewModels;

namespace AgentHarness.WinUI;

public sealed partial class SkillsPage : Page
{
    public SkillsViewModel ViewModel { get; }

    public SkillsPage(SkillsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
