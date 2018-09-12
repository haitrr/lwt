using Lwt.Interfaces;
using System;
using System.Security.Claims;

namespace Lwt.Utilities
{
    public class AuthenticationHelper : IAuthenticationHelper
    {
        public Guid GetLoggedInUser(ClaimsPrincipal principal)
        {
            return new Guid(principal.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
