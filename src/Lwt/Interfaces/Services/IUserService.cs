namespace Lwt.Interfaces.Services
{
    using System.Threading.Tasks;

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
        Task<bool> LoginAsync(string userName, string password);

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task LogoutAsync();
    }
}
