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
    public ICollection<LanguageSetting> LanguageSettings { get; set; } = new List<LanguageSetting>();
  }
}