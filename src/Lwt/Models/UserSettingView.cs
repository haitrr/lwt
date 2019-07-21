namespace Lwt.Models
{
    using System;
    using System.Collections.Generic;

    public class UserSettingView
    {
        public Guid UserId { get; set; }
        public ICollection<LanguageSetting> LanguageSettings { get; set; }
    }
}