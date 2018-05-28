using System;
using System.Security.Principal;

namespace Lwt.Interfaces
{
    public interface IAuthenticationHelper
    {
        Guid GetLoggedInUser(IIdentity identity);
    }
}
