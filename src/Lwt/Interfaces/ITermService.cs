namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Models;
    using Lwt.ViewModels;

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
        Task<int> CountAsync(Guid userId, TermFilter termFilter);

        /// <summary>
        /// get meaning of a user's term.
        /// </summary>
        /// <param name="userId"> user id.</param>
        /// <param name="termId">term id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TermMeaningDto> GetMeaningAsync(Guid userId, Guid termId);
    }
}