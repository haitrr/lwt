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
        public string Name { get; set; } = "English";

        /// <inheritdoc/>
        public Language Id { get; set; } = Language.English;

        /// <inheritdoc />
        public bool ShouldSkip(string term)
        {
            return Regex.IsMatch(term, @"([^a-zA-Z0-9\’\'])");
        }

        /// <inheritdoc/>
        public string[] SplitText(string text)
        {
            return Regex.Split(text, @"([^a-zA-Z0-9\’\'])")
                .Where(word => !string.IsNullOrEmpty(word))
                .ToArray();
        }

        /// <inheritdoc />
        public string Normalize(string word)
        {
            return word.ToUpperInvariant();
        }
    }
}