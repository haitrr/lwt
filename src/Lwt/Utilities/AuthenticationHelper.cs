namespace Lwt.Utilities;

using System.Security.Claims;

using Lwt.Interfaces;

/// <summary>
/// AuthenticationHelper.
/// </summary>
public class AuthenticationHelper : IAuthenticationHelper
{
    /// <inheritdoc/>
    public int GetLoggedInUser(ClaimsPrincipal principal)
    {
        string userId = principal.FindFirstValue(Constants.UserIdClaimType);

        return int.Parse(userId);
    }
}