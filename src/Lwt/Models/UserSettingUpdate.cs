namespace Lwt.Models
{
    using System.Collections.Generic;

    public class UserSettingUpdate
    {
        public ICollection<LanguageSetting> LanguageSettings { get; set; }
    }
}