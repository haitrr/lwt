namespace Lwt.Test.Utilities;

using System.Collections.Generic;
using System.Security.Claims;
using Lwt.Utilities;
using Xunit;

/// <summary>
/// test authentication helper.
/// </summary>
public class AuthenticationHelperTest
{
    private readonly AuthenticationHelper authenticationHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationHelperTest"/> class.
    /// </summary>
    public AuthenticationHelperTest()
    {
        this.authenticationHelper = new AuthenticationHelper();
    }

    /// <summary>
    /// get user id should work.
    /// </summary>
    [Fact]
    public void GetLoggedInUserShouldWork()
    {
        var userId = 1;
        var principal = new ClaimsPrincipal(
            new[]
            {
                new ClaimsIdentity(
                    new List<Claim>
                    {
                        new Claim("id", userId.ToString()),
                    }),
            });
        int actual = this.authenticationHelper.GetLoggedInUser(principal);

        Assert.Equal(userId, actual);
    }
}