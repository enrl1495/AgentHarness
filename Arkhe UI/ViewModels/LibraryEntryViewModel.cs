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
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<string> Tags { get; set; } = new();

    [ObservableProperty]
    public partial double RelevanceScore { get; set; }

    [ObservableProperty]
    public partial string SourcePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime LastIndexed { get; set; }
}
