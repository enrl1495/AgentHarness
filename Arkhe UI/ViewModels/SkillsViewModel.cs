using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AgentHarness.WinUI.ViewModels;

/// <summary>
/// Skills page view model managing skill discovery and toggle state.
/// </summary>
public partial class SkillsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<SkillViewModel> _skills = new();

    public SkillsViewModel()
    {
        LoadSkills();
    }

    [RelayCommand]
    private void ToggleSkill(SkillViewModel skill)
    {
        skill.IsEnabled = !skill.IsEnabled;
    }

    private void LoadSkills()
    {
        Skills.Clear();
        var skillsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgentHarness",
            "skills");

        if (!Directory.Exists(skillsPath))
        {
            try
            {
                Directory.CreateDirectory(skillsPath);
            }
            catch
            {
                // Ignore creation failures - show empty state
                return;
            }
        }

        var mdFiles = Directory.GetFiles(skillsPath, "*.md");
        foreach (var file in mdFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var firstLine = File.ReadLines(file).FirstOrDefault() ?? string.Empty;

            Skills.Add(new SkillViewModel
            {
                Name = name,
                Description = firstLine.Length > 80 ? firstLine[..80] + "..." : firstLine,
                FilePath = file,
                IsEnabled = true,
                Category = "custom"
            });
        }
    }
}
