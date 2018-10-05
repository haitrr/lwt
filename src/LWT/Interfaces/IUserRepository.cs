namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;

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
    }
}