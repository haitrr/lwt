namespace Lwt.Utilities
{
    using System;
    using System.Security.Claims;
    using Lwt.Interfaces;

    /// <summary>
    /// AuthenticationHelper.
    /// </summary>
    public class AuthenticationHelper : IAuthenticationHelper
    {
        /// <inheritdoc/>
        public Guid GetLoggedInUser(ClaimsPrincipal principal)
        {
            return new Guid(principal.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
