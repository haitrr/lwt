namespace Lwt.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;

/// <summary>
/// sql text repo.
/// </summary>
public interface ISqlTextRepository : ISqlRepository<Text>
{
    /// <summary>
    /// a.
    /// </summary>
    /// <param name="userId">userId.</param>
    /// <param name="textFilter">the filters.</param>
    /// <param name="paginationQuery">the pagination query.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<Text>> GetByUserAsync(int userId, TextFilter textFilter, PaginationQuery paginationQuery);

    /// <summary>
    /// count the text from the user that match the filters.
    /// </summary>
    /// <param name="userId">the user id.</param>
    /// <param name="textFilter">the filters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<long> CountByUserAsync(int userId, TextFilter textFilter);

    void UpdateBookmarkAsync(int id, ulong? bookMark);

    void UpdateProcessedTermCount(Text text);

    void UpdateTermCountAndProcessedTermCount(Text processingText);

    void UpdateTextLastReadAt(Text text);
}