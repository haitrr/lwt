namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Lwt.Interfaces;

    /// <inheritdoc />
    public class TextSplitter : ITextSplitter
    {
        /// <inheritdoc />
        public ICollection<string> SplitKeepDelimiter(string text, string delimiterPattern)
        {
            return Regex.Split(text, $"(\\{delimiterPattern})");
        }
    }
}