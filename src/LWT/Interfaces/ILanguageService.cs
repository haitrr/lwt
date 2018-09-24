namespace Lwt.Interfaces
{
    using System;
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
        Task<Guid> CreateAsync(Guid creatorId, LanguageCreateModel languageCreateModel);
    }
}