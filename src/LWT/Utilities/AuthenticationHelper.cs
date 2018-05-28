using Lwt.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Security.Principal;

namespace Lwt.Utilities
{
    public class AuthenticationHelper : IAuthenticationHelper
    {
        public Guid GetLoggedInUser(IIdentity identity)
        {
            return new Guid(identity.GetUserId());
        }
    }
}
