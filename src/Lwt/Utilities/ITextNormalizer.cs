namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using Lwt.Models;

    /// <summary>
    /// text normalizer.
    /// </summary>
    public interface ITextNormalizer
    {
        /// <summary>
        /// normalize a string base on its language.
        /// </summary>
        /// <param name="text">the text.</param>
        /// <param name="language">the language.</param>
        /// <returns>the normalized text.</returns>
        string Normalize(string text, Language language);

        /// <summary>
        /// normalize a strings base on its language.
        /// </summary>
        /// <param name="text">the texts.</param>
        /// <param name="language">the languages.</param>
        /// <returns>normalized texts.</returns>
        IEnumerable<string> Normalize(IEnumerable<string> text, Language language);
    }
}