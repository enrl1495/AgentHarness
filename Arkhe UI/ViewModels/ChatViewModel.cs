using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Chat page view model managing messages, input, and processing state.
/// </summary>
public partial class ChatViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _messages = new();

    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _thoughtLog = new();

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private double _contextUsage;

    /// <summary>
    /// Sends the current input text as a user message.
    /// Placeholder - no real LLM connection yet.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        // Add user message
        Messages.Add(new ChatMessageViewModel
        {
            Text = InputText,
            IsUser = true,
            Timestamp = DateTime.Now
        });

        InputText = string.Empty;
        IsProcessing = true;

        // Placeholder: simulate assistant response
        // TODO: Connect to real LLM service
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(InputText) && !IsProcessing;

    /// <summary>
    /// Placeholder command for file attachment.
    /// </summary>
    [RelayCommand]
    private void AttachFile()
    {
        // TODO: Implement file picker and attachment logic
    }

    /// <summary>
    /// Placeholder command for voice input.
    /// </summary>
    [RelayCommand]
    private void StartVoice()
    {
        // TODO: Implement voice recording logic
    }
}
