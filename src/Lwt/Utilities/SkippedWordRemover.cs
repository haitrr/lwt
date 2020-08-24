namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class SkippedWordRemover : ISkippedWordRemover
    {
        private readonly ILanguageHelper languageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkippedWordRemover"/> class.
        /// </summary>
        /// <param name="languageHelper">the language helper.</param>
        public SkippedWordRemover(ILanguageHelper languageHelper)
        {
            this.languageHelper = languageHelper;
        }

        /// <inheritdoc/>
        public IEnumerable<string> RemoveSkippedWords(IEnumerable<string> words, LanguageCode languageCode)
        {
            ILanguage lang = this.languageHelper.GetLanguage(languageCode);
            return words.Where(word => !lang.ShouldSkip(word));
        }
    }
}