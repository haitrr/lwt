namespace Lwt.Interfaces
{
    using System.Collections.Generic;

    using Lwt.Models;

    /// <summary>
    /// the language helper.
    /// </summary>
    public interface ILanguageHelper
    {
        /// <summary>
        /// get a language.
        /// </summary>
        /// <param name="language">the language.</param>
        /// <returns>the language object.</returns>
        ILanguage GetLanguage(Language language);

        /// <summary>
        /// get all the languages.
        /// </summary>
        /// <returns>all supported language.</returns>
        ICollection<ILanguage> GetAllLanguages();
    }
}