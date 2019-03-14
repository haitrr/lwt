namespace Lwt.Models
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Lwt.Interfaces;

    /// <summary>
    /// Japanese language.
    /// </summary>
    public class Japanese : ILanguage
    {
        private readonly IJapaneseTextSplitter textSplitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Japanese"/> class.
        /// </summary>
        /// <param name="textSplitter">the text splitter.</param>
        public Japanese(IJapaneseTextSplitter textSplitter)
        {
            this.textSplitter = textSplitter;
        }

        /// <inheritdoc/>
        public string Name => "Japanese";

        /// <inheritdoc />
        public string SpeakCode => "ja-JP";

        /// <inheritdoc />
        public string Code => "ja";

        /// <inheritdoc/>
        public Language Id => Language.Japanese;

        /// <inheritdoc />
        public bool ShouldSkip(string term)
        {
            return !Regex.IsMatch(term, "^\\p{Lo}+$");
        }

        /// <inheritdoc />
        public string[] SplitText(string text)
        {
            return this.textSplitter.Split(text).ToArray();
        }

        /// <inheritdoc />
        public string Normalize(string word)
        {
            return word;
        }
    }
}