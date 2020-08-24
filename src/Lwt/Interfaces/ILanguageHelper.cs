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
        /// <param name="languageCode"></param>
        /// <returns>the language object.</returns>
        ILanguage GetLanguage(LanguageCode languageCode);

        /// <summary>
        /// get all the languages.
        /// </summary>
        /// <returns>all supported language.</returns>
        ICollection<ILanguage> GetAllLanguages();
    }
}