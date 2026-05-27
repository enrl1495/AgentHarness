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
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _isUser;

    [ObservableProperty]
    private DateTime _timestamp = DateTime.Now;

    [ObservableProperty]
    private int? _latencyMs;

    [ObservableProperty]
    private int? _tokensUsed;

    [ObservableProperty]
    private string? _thoughtContent;

    [ObservableProperty]
    private bool _isThoughtExpanded;

    /// <summary>
    /// Command to toggle the thought content expansion state.
    /// </summary>
    [RelayCommand]
    private void ToggleThought()
    {
        IsThoughtExpanded = !IsThoughtExpanded;
    }
}
