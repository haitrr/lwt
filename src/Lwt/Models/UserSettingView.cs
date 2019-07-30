namespace Lwt.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// User setting view.
    /// </summary>
    public class UserSettingView
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the language settings.
        /// </summary>
        public IDictionary<string, LanguageSetting> LanguageSettings { get; set; }
    }
}