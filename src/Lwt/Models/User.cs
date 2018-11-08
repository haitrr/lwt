namespace Lwt.Models
{
    using System;

    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// User.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Gets or sets current language.
        /// </summary>
        public Guid? CurrentLanguageId { get; set; }

        /// <summary>
        /// Gets or sets current language.
        /// </summary>
        public Language CurrentLanguage { get; set; }
    }
}