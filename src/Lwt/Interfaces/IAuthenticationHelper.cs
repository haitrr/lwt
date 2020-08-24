namespace Lwt.Interfaces
{
    using System.Security.Claims;

    /// <summary>
    /// a.
    /// </summary>
    public interface IAuthenticationHelper
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <param name="principal">principal.</param>
        /// <returns>id of current user.</returns>
        int GetLoggedInUser(ClaimsPrincipal principal);
    }
}
