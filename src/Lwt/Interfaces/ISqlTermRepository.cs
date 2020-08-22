namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Models;

    /// <summary>
    /// sql term repository.
    /// </summary>
    public interface ISqlTermRepository : ISqlRepository<Term>
    {
        /// <summary>
        /// get many terms by contents.
        /// </summary>
        /// <param name="creatorId">the creator of term.</param>
        /// <param name="languageCode">the language code.</param>
        /// <param name="terms">the terms to find.</param>
        /// <returns>dictionary of term by content.</returns>
        Task<IDictionary<string, Term>> GetManyAsync(Guid creatorId, LanguageCode languageCode, HashSet<string> terms);

        /// <summary>
        /// get term of the user.
        /// </summary>
        /// <param name="termId">term id.</param>
        /// <param name="userId">user id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<Term> GetUserTermAsync(Guid termId, Guid userId);
    }
}