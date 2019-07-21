namespace Lwt.Models
{
    using System;
    using System.Collections.Generic;

    public class UserSetting : Entity
    {
        public Guid UserId { get; set; }

        public ICollection<LanguageSetting> LanguageSettings { get; set; }
    }
}