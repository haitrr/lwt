using Microsoft.AspNetCore.Http;

namespace Lwt.Utilities;

using System.Security.Claims;

using Lwt.Interfaces;

/// <summary>
/// AuthenticationHelper.
/// </summary>
public class AuthenticationHelper : IAuthenticationHelper
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuthenticationHelper(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public int GetLoggedInUserId()
    {
        string userId = this.httpContextAccessor.HttpContext!.User.FindFirstValue(Constants.UserIdClaimType);

        return int.Parse(userId);
    }

    public string GetLoggedInUserName()
    {
        return this.httpContextAccessor.HttpContext!.User.FindFirstValue(Constants.UserNameClaimType);
    }
}