namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public interface ITermService
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <param name="term">term.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> CreateAsync(Term term);

        /// <summary>
        /// edit a term.
        /// </summary>
        /// <param name="termEditModel">the term edit model.</param>
        /// <param name="termId">the term id want to edit.</param>
        /// <param name="userId">the user id that is trying to modify the term.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EditAsync(TermEditModel termEditModel, Guid termId, Guid userId);
    }
}