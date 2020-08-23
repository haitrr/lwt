namespace Lwt.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// user setting.
    /// </summary>
    public class UserSetting : Entity
    {
        /// <summary>
        /// Gets or sets user id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///  Gets or sets language settings.
        /// </summary>
        public IDictionary<string, LanguageSetting> LanguageSettings { get; set; } = new Dictionary<string, LanguageSetting>();
    }
}