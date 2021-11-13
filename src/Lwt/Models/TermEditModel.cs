namespace Lwt.Models;

/// <summary>
/// term edit model.
/// </summary>
public class TermEditModel
{
    /// <summary>
    /// Gets or sets meaning.
    /// </summary>
    public string Meaning { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the learning level.
    /// </summary>
    public LearningLevel LearningLevel { get; set; } = null!;
}