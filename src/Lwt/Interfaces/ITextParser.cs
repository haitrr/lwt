namespace Lwt.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// the text parser.
    /// </summary>
    public interface ITextParser
    {
        /// <summary>
        /// parse the text to get the terms.
        /// </summary>
        /// <param name="text">the text.</param>
        /// <param name="language">the language.</param>
        /// <returns>the terms.</returns>
        Task<ICollection<string>> ParseAsync(Text text, Language language);
    }
}