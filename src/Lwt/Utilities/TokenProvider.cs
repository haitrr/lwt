namespace Lwt.Utilities;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Lwt.Interfaces;
using Lwt.Models;

using Microsoft.IdentityModel.Tokens;

/// <inheritdoc />
public class TokenProvider : ITokenProvider
{
    private readonly AppSettings appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenProvider"/> class.
    /// </summary>
    /// <param name="appSettings">the application settings.</param>
    public TokenProvider(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    /// <inheritdoc />
    public string GenerateUserToken(User user)
    {
        // authentication successful so generate jwt token
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(this.appSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim(Constants.UserIdClaimType, user.Id.ToString()),
                    new Claim(Constants.UserNameClaimType, user.UserName),
                }),
            Expires = DateTime.UtcNow.AddMonths(3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}