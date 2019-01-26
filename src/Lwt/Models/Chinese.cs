namespace Lwt.Models
{
    using System.Linq;
    using Lwt.Interfaces;

    /// <summary>
    /// chinese language support.
    /// </summary>
    public class Chinese : ILanguage
    {
        private readonly IChineseTextSplitter textSplitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chinese"/> class.
        /// </summary>
        /// <param name="textSplitter">the text splitter.</param>
        public Chinese(IChineseTextSplitter textSplitter)
        {
            this.textSplitter = textSplitter;
        }

        /// <inheritdoc />
        public string Name => "Chinese";

        /// <inheritdoc />
        public string Code => "zh";

        /// <inheritdoc />
        public Language Id => Language.Chinese;

        /// <inheritdoc />
        public bool ShouldSkip(string term)
        {
            return !term.Any(c => (uint)c >= 0x4E00 && (uint)c <= 0x2FA1F);
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