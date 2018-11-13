namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public interface ITextRepository : IRepository<Text>
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <param name="textFilter">the filters.</param>
        /// <param name="paginationQuery">the pagination query.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<Text>> GetByUserAsync(Guid userId, TextFilter textFilter, PaginationQuery paginationQuery);

        /// <summary>
        /// count the text from the user that match the filters.
        /// </summary>
        /// <param name="userId">the user id.</param>
        /// <param name="textFilter">the filters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<int> CountByUserAsync(Guid userId, TextFilter textFilter);
    }
}