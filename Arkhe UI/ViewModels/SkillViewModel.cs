using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Represents a single skill loaded from the skills directory.
/// </summary>
public partial class SkillViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private string _category = string.Empty;

    [RelayCommand]
    private void Toggle()
    {
        IsEnabled = !IsEnabled;
    }
}
