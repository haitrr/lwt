namespace Lwt.Services;

using System.Threading.Tasks;

/// <summary>
/// user password changer.
/// </summary>
public interface IUserPasswordChanger
{
    /// <summary>
    /// change the user password.
    /// </summary>
    /// <param name="userId">the target user id.</param>
    /// <param name="currentPassword">current password.</param>
    /// <param name="newPassword">new password.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}