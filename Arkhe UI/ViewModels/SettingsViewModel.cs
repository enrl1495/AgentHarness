using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Settings page view model managing theme, model, and endpoint configuration.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _theme = "Dark";

    [ObservableProperty]
    private string _model = "deepseek-v4-pro";

    [ObservableProperty]
    private string _endpoint = "http://localhost:11434";

    [ObservableProperty]
    private bool _isMicaEnabled = true;

    [ObservableProperty]
    private double _micaOpacity = 0.7;

    [ObservableProperty]
    private string _accentColor = "#FF8C42";

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
