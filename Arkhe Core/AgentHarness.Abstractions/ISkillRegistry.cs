namespace AgentHarness.Abstractions;

/// <summary>
/// Defines a contract for managing dynamic skill loading from the local AppData directory.
/// </summary>
public interface ISkillRegistry
{
    /// <summary>
    /// Gets the file system path where skills are stored.
    /// </summary>
    /// <returns>The full path to the skills folder.</returns>
    string GetSkillsFolderPath();

    /// <summary>
    /// Gets the list of all local skills with their current enabled/disabled status.
    /// </summary>
    /// <returns>A list of skill information records.</returns>
    List<SkillInfo> GetSkills();

    /// <summary>
    /// Toggles a skill's enabled/disabled status.
    /// </summary>
    /// <param name="name">The skill name (file name without extension).</param>
    /// <param name="isEnabled">True to enable, false to disable.</param>
    void ToggleSkill(string name, bool isEnabled);

    /// <summary>
    /// Loads only the enabled local skills and returns a combined instruction string.
    /// </summary>
    /// <returns>Combined content of all enabled skills, or empty string if none.</returns>
    string LoadEnabledSkills();
}

/// <summary>
/// Represents information about a skill.
/// </summary>
/// <param name="Name">The skill name (file name without extension).</param>
/// <param name="Path">The full file path to the skill markdown file.</param>
/// <param name="IsEnabled">Whether the skill is currently enabled.</param>
public record SkillInfo(string Name, string Path, bool IsEnabled);
