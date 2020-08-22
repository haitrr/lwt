namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class TextNormalizer : ITextNormalizer
    {
        private readonly ILanguageHelper languageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextNormalizer"/> class.
        /// </summary>
        /// <param name="languageHelper">the language helper.</param>
        public TextNormalizer(ILanguageHelper languageHelper)
        {
            this.languageHelper = languageHelper;
        }

        /// <inheritdoc/>
        public string Normalize(string text, LanguageCode languageCode)
        {
            ILanguage lang = this.languageHelper.GetLanguage(languageCode);
            return lang.Normalize(text);
        }

        /// <inheritdoc/>
        public IEnumerable<string> Normalize(IEnumerable<string> text, LanguageCode languageCode)
        {
            return text.Select(t => this.Normalize(t, (LanguageCode)languageCode));
        }
    }
}