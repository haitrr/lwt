namespace Lwt.Utilities
{
    using System;
    using System.Collections.Generic;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class LanguageHelper : ILanguageHelper
    {
        /// <inheritdoc/>
        public ILanguage GetLanguage(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return new English();
            }

            throw new NotSupportedException($"Language {language.ToString()} is not supported.");
        }

        /// <inheritdoc/>
        public ICollection<ILanguage> GetAllLanguages()
        {
            return new List<ILanguage>() { new English() };
        }
    }
}