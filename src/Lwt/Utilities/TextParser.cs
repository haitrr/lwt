namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class TextParser : ITextParser
    {
        private readonly ITextSplitter textSplitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextParser"/> class.
        /// </summary>
        /// <param name="textSplitter">the text splitter.</param>
        public TextParser(ITextSplitter textSplitter)
        {
            this.textSplitter = textSplitter;
        }

        /// <inheritdoc/>
        public async Task<ICollection<string>> ParseAsync(Text text, Language language)
        {
            ICollection<string> words = this.textSplitter.SplitKeepDelimiter(text.Content, language.DelimiterPattern);

            return await Task.FromResult(words);
        }
    }
}