namespace Lwt.Interfaces
{
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// the term repository.
    /// </summary>
    public interface ITermRepository : IRepository<Term>
    {
        /// <summary>
        /// Get a term by the content.
        /// </summary>
        /// <param name="content">the content.</param>
        /// <returns>the term.</returns>
        Task<Term> GetByContentAsync(string content);
    }
}