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
        public string Normalize(string text, Language language)
        {
            ILanguage lang = this.languageHelper.GetLanguage(language);
            return lang.Normalize(text);
        }

        /// <inheritdoc/>
        public IEnumerable<string> Normalize(IEnumerable<string> text, Language language)
        {
            return text.Select(t => this.Normalize(t, language));
        }
    }
}