namespace Lwt.Interfaces;

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
    Task<bool> IsExistAsync(int id);

    /// <summary>
    /// create a new user.
    /// </summary>
    /// <param name="user">the new user.</param>
    /// <param name="password">password.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateAsync(User user, string password);

    /// <summary>
    /// get user by id return null if user not found.
    /// </summary>
    /// <param name="userId">the user id.</param>
    /// <returns>the user.</returns>
    Task<User?> TryGetByIdAsync(int userId);

    /// <summary>
    /// get user by id, throw exception if user not found.
    /// </summary>
    /// <param name="userId">the user id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<User> GetByIdAsync(int userId);

    /// <summary>
    /// get user by user name.
    /// </summary>
    /// <param name="userName">the user name.</param>
    /// <returns>the user.</returns>
    Task<User?> GetByUserNameAsync(string userName);

    /// <summary>
    /// change the password of a user.
    /// </summary>
    /// <param name="user">the user.</param>
    /// <param name="currentPassword">current password.</param>
    /// <param name="newPassword">the new password.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword);
}