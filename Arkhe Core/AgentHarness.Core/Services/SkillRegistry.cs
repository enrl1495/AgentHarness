using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgentHarness.Abstractions;

namespace AgentHarness.Core.Services;

/// <summary>
/// Manages the dynamic loading of personal skills from the local AppData directory.
/// Implements ISkillRegistry interface for dependency injection.
/// </summary>
public class SkillRegistry : ISkillRegistry
{
    private readonly string _skillsFolder;
    private readonly HashSet<string> _disabledSkills = new();

    public SkillRegistry()
    {
        _skillsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AgentHarness", "skills");
        if (!Directory.Exists(_skillsFolder))
        {
            Directory.CreateDirectory(_skillsFolder);
            CreateDefaultSkill();
        }
    }

    public string GetSkillsFolderPath() => _skillsFolder;

    /// <summary>
    /// Gets the list of all local skills with their current status.
    /// </summary>
    public List<SkillInfo> GetSkills()
    {
        try
        {
            var files = Directory.GetFiles(_skillsFolder, "*.md");
            return files.Select(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                return new SkillInfo(name, f, !_disabledSkills.Contains(name));
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[{nameof(SkillRegistry)}.{nameof(GetSkills)}] {ex.GetType().Name}: {ex.Message}");
            return new List<SkillInfo>();
        }
    }

    public void ToggleSkill(string name, bool isEnabled)
    {
        if (isEnabled) _disabledSkills.Remove(name);
        else _disabledSkills.Add(name);
    }

    /// <summary>
    /// Loads only the ENABLED local skills and returns a combined instruction string.
    /// </summary>
    public string LoadEnabledSkills()
    {
        try
        {
            var skills = GetSkills().Where(s => s.IsEnabled).ToList();
            if (!skills.Any()) return string.Empty;

            var result = "\n--- PERSONAL SKILLS ---\n";
            foreach (var skill in skills)
            {
                var content = File.ReadAllText(skill.Path);
                result += $"[Skill: {skill.Name}]\n{content}\n\n";
            }
            return result;
        }
        catch (Exception ex)
        {
            return $"\n[Error loading skills: {ex.Message}]\n";
        }
    }

    private void CreateDefaultSkill()
    {
        var path = Path.Combine(_skillsFolder, "CleanCodeReviewer.md");
        if (!File.Exists(path))
        {
            var content = @"# Clean Code Reviewer
Cuando revises código o escribas código nuevo, sigue estas reglas:
1. Usa nombres descriptivos para las variables.
2. Mantén las funciones cortas y enfocadas en una sola tarea.
3. Prioriza la legibilidad sobre la optimización prematura.
4. Siempre agrega comentarios XML a las clases y métodos públicos.";
            File.WriteAllText(path, content);
        }
    }
}

