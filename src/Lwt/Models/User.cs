namespace Lwt.Models
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// User.
    /// </summary>
    public class User : IdentityUser<int>
    {
        /// <summary>
        /// Gets or sets current language.
        /// </summary>
        public int? CurrentLanguageId { get; set; }

        public IList<Term> Terms { get; set; } = new List<Term>();

        public IList<Text> Text { get; set; } = new List<Text>();

        public UserSetting Setting { get; set; } = new UserSetting();
    }
}