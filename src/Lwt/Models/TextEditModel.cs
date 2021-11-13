namespace Lwt.Models;

/// <summary>
/// language edit model.
/// </summary>
public class TextEditModel
{
    /// <summary>
    /// Gets or sets text language.
    /// </summary>
    public LanguageCode LanguageCode { get; set; } = LanguageCode.ENGLISH;

    /// <summary>
    /// Gets or sets title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}