namespace Lwt.Utilities
{
    using System;
    using System.Collections.Generic;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class TextSeparator : ITextSeparator
    {
        private readonly ILanguageHelper languageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextSeparator"/> class.
        /// </summary>
        /// <param name="languageHelper">the language helper.</param>
        public TextSeparator(ILanguageHelper languageHelper)
        {
            this.languageHelper = languageHelper;
        }

        /// <inheritdoc />
        public IEnumerable<string> SeparateText(string text, Language language)
        {
            try
            {
                ILanguage lang = this.languageHelper.GetLanguage(language);
                return lang.SplitText(text);
            }
            catch (NotSupportedException exception)
            {
                throw new NotSupportedException(exception.Message);
            }
        }
    }
}