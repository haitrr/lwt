namespace Lwt.Models;

/// <summary>
/// application setting.
/// </summary>
public record AppSettings
{
    /// <summary>
    /// Gets or sets the application secret key.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    public string JapaneseSegmenterEndpoint { get; set; } = string.Empty;
}