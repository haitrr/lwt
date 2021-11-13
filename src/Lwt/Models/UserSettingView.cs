namespace Lwt.Models;

using System.Collections.Generic;

/// <summary>
/// User setting view.
/// </summary>
public class UserSettingView
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the language settings.
    /// </summary>
    public ICollection<LanguageSetting> LanguageSettings { get; set; } =
        new List<LanguageSetting>();
}