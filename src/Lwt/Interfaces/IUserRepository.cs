namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// repository for user.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// is check if a user with id is existed.
        /// </summary>
        /// <param name="id">id of the user.</param>
        /// <returns>whether the user is exist or not.</returns>
        Task<bool> IsExistAsync(Guid id);

        /// <summary>
        /// create a new user.
        /// </summary>
        /// <param name="user">the new user.</param>
        /// <param name="password">password.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateAsync(User user, string password);

        /// <summary>
        /// get user by id.
        /// </summary>
        /// <param name="userId">the user id.</param>
        /// <returns>the user.</returns>
        Task<User> GetByIdAsync(Guid userId);
    }
}