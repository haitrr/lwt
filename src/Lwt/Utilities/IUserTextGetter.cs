namespace Lwt.Utilities;

using System.Threading.Tasks;
using Lwt.Models;

/// <summary>
/// get text of a user.
/// </summary>
public interface IUserTextGetter
{
    /// <summary>
    /// get text of a user.
    /// </summary>
    /// <param name="textId">text id.</param>
    /// <param name="userId">user id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<Text> GetUserTextAsync(int textId, int userId);
}