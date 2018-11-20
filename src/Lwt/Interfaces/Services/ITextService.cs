namespace Lwt.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public interface ITextService
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <param name="text">text.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateAsync(Text text);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <param name="textFilter">the filters.</param>
        /// <param name="paginationQuery">the pagination query.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<Text>> GetByUserAsync(Guid userId, TextFilter textFilter, PaginationQuery paginationQuery);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="userId">userId.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteAsync(Guid id, Guid userId);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="textId">textId.</param>
        /// <param name="userId">userId.</param>
        /// <param name="editModel">editModel.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task EditAsync(Guid textId, Guid userId, TextEditModel editModel);

        /// <summary>
        /// count the ad from the user that match the filters.
        /// </summary>
        /// <param name="userId">the user id.</param>
        /// <param name="textFilter">the filters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> CountAsync(Guid userId, TextFilter textFilter);
    }
}