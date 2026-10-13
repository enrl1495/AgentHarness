using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Represents a single chat message in the conversation.
/// </summary>
public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUser { get; set; }

    [ObservableProperty]
    public partial DateTime Timestamp { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial int? LatencyMs { get; set; }

    [ObservableProperty]
    public partial int? TokensUsed { get; set; }

    [ObservableProperty]
    public partial string? ThoughtContent { get; set; }

    [ObservableProperty]
    public partial bool IsThoughtExpanded { get; set; }

    /// <summary>
    /// Command to toggle the thought content expansion state.
    /// </summary>
    [RelayCommand]
    private void ToggleThought()
    {
        IsThoughtExpanded = !IsThoughtExpanded;
    }
}
