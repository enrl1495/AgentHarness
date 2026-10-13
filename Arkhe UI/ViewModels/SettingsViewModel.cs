using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Settings page view model managing theme, model, and endpoint configuration.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Theme { get; set; } = "Dark";

    [ObservableProperty]
    public partial string Model { get; set; } = "deepseek-v4-pro";

    [ObservableProperty]
    public partial string Endpoint { get; set; } = "http://localhost:11434";

    [ObservableProperty]
    public partial bool IsMicaEnabled { get; set; } = true;

    [ObservableProperty]
    public partial double MicaOpacity { get; set; } = 0.7;

    [ObservableProperty]
    public partial string AccentColor { get; set; } = "#FF8C42";

    /// <summary>
    /// Saves current settings (placeholder - no persistence yet).
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        // TODO: Implement settings persistence
        System.Diagnostics.Debug.WriteLine($"Settings saved: Theme={Theme}, Model={Model}, Endpoint={Endpoint}");
    }

    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    [RelayCommand]
    private void Reset()
    {
        Theme = "Dark";
        Model = "deepseek-v4-pro";
        Endpoint = "http://localhost:11434";
        IsMicaEnabled = true;
        MicaOpacity = 0.7;
        AccentColor = "#FF8C42";
    }
}
