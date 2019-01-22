namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Models;

    /// <summary>
    /// the term repository.
    /// </summary>
    public interface ITermRepository : IRepository<Term>
    {
        /// <summary>
        /// get the term by the creator id and the content.
        /// </summary>
        /// <param name="userId">the creator id.</param>
        /// <param name="language">the term's language.</param>
        /// <param name="word">the content.</param>
        /// <returns>the term.</returns>
        Task<Term> GetByUserAndLanguageAndContentAsync(Guid userId, Language language, string word);

        /// <summary>
        /// get learning level of the terms with content.
        /// </summary>
        /// <param name="creatorId">the creator of term.</param>
        /// <param name="language">the language.</param>
        /// <param name="terms">the terms to find.</param>
        /// <returns>dictionary of term learning level.</returns>
        Task<Dictionary<string, TermLearningLevel>> GetLearningLevelAsync(
            Guid creatorId,
            Language language,
            ISet<string> terms);

        /// <summary>
        /// get many terms by contents.
        /// </summary>
        /// <param name="creatorId">the creator of term.</param>
        /// <param name="language">the language.</param>
        /// <param name="terms">the terms to find.</param>
        /// <returns>dictionary of term by content.</returns>
        Task<IDictionary<string, Term>> GetManyAsync(Guid creatorId, Language language, HashSet<string> terms);
    }
}