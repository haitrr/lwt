namespace Lwt.Models
{
  using System.Collections.Generic;

  /// <summary>
  /// user setting update model.
  /// </summary>
  public class UserSettingUpdate
  {
    /// <summary>
    /// Gets or sets language setting.
    /// </summary>
    public IDictionary<string, LanguageSetting> LanguageSettings { get; set; } = new Dictionary<string, LanguageSetting>();
  }
}