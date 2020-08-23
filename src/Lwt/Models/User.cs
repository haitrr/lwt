namespace Lwt.Models
{
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
    }
}