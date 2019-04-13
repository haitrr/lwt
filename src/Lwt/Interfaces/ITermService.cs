namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// get a term.
        /// </summary>
        /// <param name="userId">the user id is requesting.</param>
        /// <param name="termId">the term id.</param>
        /// <returns>the term view model.</returns>
        Task<TermViewModel> GetAsync(Guid userId, Guid termId);

        /// <summary>
        /// return count of terms match filter.
        /// </summary>
        /// <param name="userId">the user.</param>
        /// <param name="termFilter">the filter.</param>
        /// <returns>count.</returns>
        Task<ulong> CountAsync(Guid userId, TermFilter termFilter);

        /// <summary>
        /// return terms that match filter with pagination.
        /// </summary>
        /// <param name="userId">id of request user.</param>
        /// <param name="termFilter">the filter.</param>
        /// <param name="paginationQuery">the pagination query.</param>
        /// <returns>list of term view models that match filter.</returns>
        Task<IEnumerable<TermViewModel>> SearchAsync(
            Guid userId,
            TermFilter termFilter,
            PaginationQuery paginationQuery);
    }
}