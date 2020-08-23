namespace Lwt.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// d.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// z.
        /// </summary>
        /// <param name="creatorId">creatorId.</param>
        /// <param name="languageCreateModel">languageCreateModel.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<int> CreateAsync(int creatorId, LanguageCreateModel languageCreateModel);

        /// <summary>
        /// get all the language of a user.
        /// </summary>
        /// <param name="userId">the user's id.</param>
        /// <returns>list of the user's language.</returns>
        Task<ICollection<Language>> GetByUserAsync(int userId);
    }
}