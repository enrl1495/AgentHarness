using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Represents a single library entry (RAG document) in the GridView.
/// </summary>
public partial class LibraryEntryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _tags = new();

    [ObservableProperty]
    private double _relevanceScore;

    [ObservableProperty]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    private DateTime _lastIndexed;
}
