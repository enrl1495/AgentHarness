using AgentHarness.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AgentHarness.WinUI;

/// <summary>
/// Chat page with message bubbles, thought log sidebar, and input area.
/// </summary>
public sealed partial class ChatPage : Page, INotifyPropertyChanged
{
    public ChatViewModel ViewModel { get; }

    private bool _isThoughtPanelVisible = true;

    /// <summary>
    /// Controls visibility of the thought log sidebar.
    /// </summary>
    public bool IsThoughtPanelVisible
    {
        get => _isThoughtPanelVisible;
        set
        {
            if (_isThoughtPanelVisible != value)
            {
                _isThoughtPanelVisible = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Constructor accepting ChatViewModel via dependency injection.
    /// </summary>
    /// <param name="viewModel">The chat view model.</param>
    public ChatPage(ChatViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    /// <summary>
    /// Collapses the thought log sidebar.
    /// </summary>
    private void CollapseThoughtPanel_Click(object sender, RoutedEventArgs e)
    {
        IsThoughtPanelVisible = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
