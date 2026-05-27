using Microsoft.UI.Xaml.Controls;
using AgentHarness.WinUI.ViewModels;

namespace AgentHarness.WinUI;

public sealed partial class LibraryPage : Page
{
    public LibraryViewModel ViewModel { get; }

    public LibraryPage(LibraryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
