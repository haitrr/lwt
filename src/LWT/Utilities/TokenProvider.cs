namespace Lwt.Utilities
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    /// <inheritdoc />
    public class TokenProvider : ITokenProvider
    {
        private readonly AppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProvider"/> class.
        /// </summary>
        /// <param name="appSettings">the application settings.</param>
        public TokenProvider(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        /// <inheritdoc />
        public string GenerateUserToken(User user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(this.appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(Constants.UserIdClaimType, user.Id.ToString()),
                    new Claim("userName", user.UserName),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}