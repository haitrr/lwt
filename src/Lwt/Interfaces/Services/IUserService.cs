namespace Lwt.Interfaces.Services;

using System.Threading.Tasks;
using Lwt.Models;

/// <summary>
/// a.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// a.
    /// </summary>
    /// <param name="userName">userName.</param>
    /// <param name="passWord">passWord.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task SignUpAsync(string userName, string passWord);

    /// <summary>
    /// a.
    /// </summary>
    /// <param name="userName">userName.</param>
    /// <param name="password">password.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> LoginAsync(string userName, string password);

    /// <summary>
    /// change user password.
    /// </summary>
    /// <param name="userId">user id.</param>
    /// <param name="changePasswordModel">change password model.</param>
    /// <returns>nothing.</returns>
    Task ChangePasswordAsync(int userId, UserChangePasswordModel changePasswordModel);

    /// <summary>
    /// get user.
    /// </summary>
    /// <param name="loggedInUserid">user id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<UserView> GetAsync(int loggedInUserid);

    /// <summary>
    /// get setting of a user.
    /// </summary>
    /// <param name="loggedInUserid">user id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<UserSettingView> GetSettingAsync(int loggedInUserid);

    /// <summary>
    /// update setting of a user.
    /// </summary>
    /// <param name="loggedInUserid"> user id.</param>
    /// <param name="userSettingUpdate">settings.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task PutSettingAsync(int loggedInUserid, UserSettingUpdate userSettingUpdate);
}