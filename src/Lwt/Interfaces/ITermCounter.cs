namespace Lwt.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Models;

    /// <summary>
    /// the term counter.
    /// </summary>
    public interface ITermCounter
    {
        /// <summary>
        /// count the terms by learning level.
        /// </summary>
        /// <param name="words">the words.</param>
        /// <param name="languageCode">language code.</param>
        /// <param name="userId">the user id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<Dictionary<LearningLevel, long>> CountByLearningLevelAsync(
            IEnumerable<string> words,
            LanguageCode languageCode,
            int userId);

        long CountTermFromTextContent(Text text);
    }
}