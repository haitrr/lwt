using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Lwt.Interfaces
{
    public interface IAuthenticationHelper
    {
        Guid GetLoggedInUser(ClaimsPrincipal principal);
    }
}
