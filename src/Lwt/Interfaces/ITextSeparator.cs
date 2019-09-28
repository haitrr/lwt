namespace Lwt.Interfaces
{
    using System.Collections.Generic;
    using Lwt.Models;

    /// <summary>
    /// text separator.
    /// </summary>
    public interface ITextSeparator
    {
        /// <summary>
        /// separate the text base on its language.
        /// </summary>
        /// <param name="text">text.</param>
        /// <param name="language">language.</param>
        /// <returns>the words separated from the text.</returns>
        IEnumerable<string> SeparateText(string text, Language language);
    }
}