namespace Lwt.Interfaces;

using System.Security.Claims;

/// <summary>
/// a.
/// </summary>
public interface IAuthenticationHelper
{
    int GetLoggedInUserId();

    string GetLoggedInUserName();
}