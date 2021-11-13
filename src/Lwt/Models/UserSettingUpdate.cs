namespace Lwt.Models;

using System.Collections.Generic;

/// <summary>
/// user setting update model.
/// </summary>
public class UserSettingUpdate
{
    /// <summary>
    /// Gets or sets language setting.
    /// </summary>
    public IEnumerable<LanguageSetting> LanguageSettings { get; set; } = null!;
}