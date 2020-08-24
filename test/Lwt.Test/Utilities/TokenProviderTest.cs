namespace Lwt.Test.Utilities
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Utilities;
    using Xunit;

    /// <summary>
    /// test token provider.
    /// </summary>
    public class TokenProviderTest
    {
        private readonly ITokenProvider tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProviderTest"/> class.
        /// </summary>
        public TokenProviderTest()
        {
            this.tokenProvider = new TokenProvider(new AppSettings { Secret = "this secret is a secret must be longer than 128 bits." });
        }

        /// <summary>
        /// test token generator.
        /// </summary>
        [Fact]
        public void GenerateUserTokenShouldReturnRightToken()
        {
            var userId = 1;
            var userName = "yolo";
            var user = new User { Id = userId, UserName = userName };

            string token = this.tokenProvider.GenerateUserToken(user);

            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            Assert.Equal(
                userName,
                tokenS?.Claims.First(claim => claim.Type == "userName").Value);
            Assert.Equal(
                userId,
                int.Parse(
                    (ReadOnlySpan<char>)tokenS?.Claims.First(claim => claim.Type == "id")
                        .Value));
        }
    }
}