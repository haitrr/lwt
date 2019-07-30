namespace Lwt.Models
{
    using System.Collections.Generic;

    public class UserSettingUpdate
    {
        public IDictionary<string,LanguageSetting> LanguageSettings { get; set; }
    }
}