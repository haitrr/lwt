namespace Lwt.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// text splitter.
    /// </summary>
    public interface ITextSplitter
    {
        /// <summary>
        /// split a text using a delimiter pattern and keep the delimiter.
        /// </summary>
        /// <param name="text">the text.</param>
        /// <param name="delimiterPattern"> a regex expression to match the delimiters.</param>
        /// <returns>list of word and delimiter after split.</returns>
        ICollection<string> SplitKeepDelimiter(string text, string delimiterPattern);
    }
}