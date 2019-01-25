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
        /// <returns>list of word and delimiter after split.</returns>
        IEnumerable<string> Split(string text);
    }
}