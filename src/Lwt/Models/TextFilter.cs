namespace Lwt.Models;

/// <summary>
/// the text filters.
/// </summary>
public class TextFilter
{
    /// <summary>
    /// Gets or sets the language of the text.
    /// </summary>
    public LanguageCode? LanguageCode { get; set; }

    public string? Title { get; set; }
}