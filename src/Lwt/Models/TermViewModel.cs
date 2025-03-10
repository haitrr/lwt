namespace Lwt.Models;

/// <summary>
/// term view model.
/// </summary>
public class TermViewModel
{
    /// <summary>
    /// Gets or sets content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets meaning.
    /// </summary>
    public string Meaning { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets learning level.
    /// </summary>
    public LearningLevel LearningLevel { get; set; } = null!;

    /// <summary>
    /// Gets or sets language code.
    /// </summary>
    public LanguageCode LanguageCode { get; set; } = null!;

    /// <summary>
    /// Gets or sets term's id.
    /// </summary>
    public int Id { get; set; }
}