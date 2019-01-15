namespace Lwt.Interfaces
{
    using System;
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
    }
}