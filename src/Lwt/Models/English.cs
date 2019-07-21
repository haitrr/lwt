namespace Lwt.Models
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Lwt.Interfaces;

    /// <summary>
    /// English language.
    /// </summary>
    public class English : ILanguage
    {
        /// <inheritdoc/>
        public string Name => "English";

        /// <inheritdoc />
        public string SpeakCode => "en-US";

        /// <inheritdoc />
        public string Code => "en";

        /// <inheritdoc/>
        public Language Id => Language.English;

        /// <inheritdoc />
        public bool ShouldSkip(string term)
        {
            return Regex.IsMatch(term, @"([^a-zA-Z\’\'])");
        }

        /// <inheritdoc/>
        public string[] SplitText(string text)
        {
            return Regex.Split(text, @"([^a-zA-Z\’\'])").Where(word => !string.IsNullOrEmpty(word)).ToArray();
        }

        /// <inheritdoc />
        public string Normalize(string word)
        {
            return word.ToUpperInvariant();
        }
    }
}