namespace Lwt.Interfaces.Services
{
    using System;
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
        Task ChangePasswordAsync(Guid userId, UserChangePasswordModel changePasswordModel);
    }
}
