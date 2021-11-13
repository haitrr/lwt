namespace Lwt.Models;

/// <summary>
/// Language view model.
/// </summary>
public class LanguageViewModel
{
    /// <summary>
    /// Gets or sets language name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language's code.
    /// </summary>
    public LanguageCode Code { get; set; } = LanguageCode.ENGLISH;

    /// <summary>
    /// Gets or sets the language's speak code.
    /// </summary>
    public string SpeakCode { get; set; } = string.Empty;
}