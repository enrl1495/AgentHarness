using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Represents a single skill loaded from the skills directory.
/// </summary>
public partial class SkillViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial string Category { get; set; } = string.Empty;

    [RelayCommand]
    private void Toggle()
    {
        IsEnabled = !IsEnabled;
    }
}
