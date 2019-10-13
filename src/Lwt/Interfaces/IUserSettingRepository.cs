namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Models;

    /// <inheritdoc/>
    public interface IUserSettingRepository : IRepository<UserSetting>
    {
    /// <summary>
    /// get a user by it's id.
    /// </summary>
    /// <param name="userId">user's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<UserSetting?> TryGetByUserIdAsync(Guid userId);
    }
}