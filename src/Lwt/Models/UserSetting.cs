namespace Lwt.Models;

using System.Collections.Generic;

/// <summary>
/// user setting.
/// </summary>
public record UserSetting : Entity
{
    public const string TableName = "user_settings";

    /// <summary>
    /// Gets or sets user id.
    /// </summary>
    public int UserId { get; set; }

    public User User { get; set; }

    /// <summary>
    ///  Gets or sets language settings.
    /// </summary>
    public ICollection<LanguageSetting> LanguageSettings { get; set; } =
        new List<LanguageSetting>();
}