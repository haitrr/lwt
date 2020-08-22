namespace Lwt.Utilities
{
    using System.Collections.Generic;
    using Lwt.Models;

    /// <summary>
    /// skipped words remover.
    /// </summary>
    public interface ISkippedWordRemover
    {
        /// <summary>
        /// remove skipped words of a languages from the words.
        /// </summary>
        /// <param name="words"> the words.</param>
        /// <param name="languageCode"></param>
        /// <returns>none skipped words.</returns>
        IEnumerable<string> RemoveSkippedWords(IEnumerable<string> words, LanguageCode languageCode);
    }
}