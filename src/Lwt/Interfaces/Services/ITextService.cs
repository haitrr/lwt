namespace Lwt.Interfaces.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Models;
    using Lwt.ViewModels;

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
        Task<int> CreateAsync(Text text);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <param name="textFilter">the filters.</param>
        /// <param name="paginationQuery">the pagination query.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<TextViewModel>> GetByUserAsync(
            int userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="userId">userId.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteAsync(int id, int userId);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="textId">textId.</param>
        /// <param name="userId">userId.</param>
        /// <param name="editModel">editModel.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task EditAsync(int textId, int userId, TextEditModel editModel);

        /// <summary>
        /// count the ad from the user that match the filters.
        /// </summary>
        /// <param name="userId">the user id.</param>
        /// <param name="textFilter">the filters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> CountAsync(int userId, TextFilter textFilter);

        /// <summary>
        /// get the text to read.
        /// </summary>
        /// <param name="id">the text id.</param>
        /// <param name="userId">the user id.</param>
        /// <returns>the text read model.</returns>
        Task<TextReadModel> ReadAsync(int id, int userId);

        /// <summary>
        /// get edit detail of a text.
        /// </summary>
        /// <param name="id">the text's id.</param>
        /// <param name="userId">the request user.</param>
        /// <returns>the text edit detail.</returns>
        Task<TextEditDetailModel> GetEditDetailAsync(int id, int userId);

        /// <summary>
        /// set user index in a text.
        /// </summary>
        /// <param name="id">the text id.</param>
        /// <param name="userId">user id.</param>
        /// <param name="setBookmarkModel">set bookmark model.</param>
        /// <returns>nothing.</returns>
        Task SetBookmarkAsync(int id, int userId, SetBookmarkModel setBookmarkModel);

        Task<IDictionary<LearningLevel, int>> GetTermCountsAsync(int id, int userId);

        Task<IEnumerable<TermReadModel>> GetTextTermsAsync(int id, int userId, int indexFrom, int indexTo);

        Task<int> GetTermCountInTextAsync(int id, int userId, int termId);
    }
}