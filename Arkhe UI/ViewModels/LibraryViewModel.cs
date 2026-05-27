using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Library page view model managing RAG document search and display.
/// </summary>
public partial class LibraryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LibraryEntryViewModel> _results = new();

    [ObservableProperty]
    private LibraryEntryViewModel? _selectedEntry;

    public LibraryViewModel()
    {
        LoadSampleData();
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            LoadSampleData();
            return;
        }

        var filtered = Results
            .Where(r => r.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                     || r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                     || r.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var temp = new ObservableCollection<LibraryEntryViewModel>(Results);
        Results.Clear();
        foreach (var item in filtered)
            Results.Add(item);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        Results.Clear();
        Results.Add(new LibraryEntryViewModel
        {
            Title = "AgentHarness Core API",
            Description = "Documentation for the core agent harness functionality and tool interfaces.",
            Tags = { "docs", "api", "core" },
            RelevanceScore = 95,
            SourcePath = "%APPDATA%/AgentHarness/docs/api.md",
            LastIndexed = DateTime.Now.AddDays(-1)
        });
        Results.Add(new LibraryEntryViewModel
        {
            Title = "Skill Development Guide",
            Description = "How to create custom skills for the AgentHarness platform.",
            Tags = { "skills", "tutorial" },
            RelevanceScore = 88,
            SourcePath = "%APPDATA%/AgentHarness/docs/skills.md",
            LastIndexed = DateTime.Now.AddDays(-2)
        });
        Results.Add(new LibraryEntryViewModel
        {
            Title = "RAG Implementation",
            Description = "Vector search and retrieval-augmented generation architecture.",
            Tags = { "rag", "architecture", "vector" },
            RelevanceScore = 72,
            SourcePath = "%APPDATA%/AgentHarness/docs/rag.md",
            LastIndexed = DateTime.Now.AddDays(-3)
        });
        Results.Add(new LibraryEntryViewModel
        {
            Title = "Testing Best Practices",
            Description = "Unit and integration testing strategies for agent tools.",
            Tags = { "testing", "best-practices" },
            RelevanceScore = 65,
            SourcePath = "%APPDATA%/AgentHarness/docs/testing.md",
            LastIndexed = DateTime.Now.AddDays(-5)
        });
    }
}
