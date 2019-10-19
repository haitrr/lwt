namespace Lwt.Test.IntegrationTests
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Models;
    using Lwt.ViewModels.User;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// test user apis integration.
    /// </summary>
    public class UserIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient client;
        private readonly CustomWebApplicationFactory<Startup> factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIntegrationTest"/> class.
        /// </summary>
        /// <param name="factory">the web host factory.</param>
        public UserIntegrationTest(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
            this.client = this.factory.CreateClient();
        }

        /// <summary>
        /// should be able to sign up.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUpShouldWork()
        {
            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                identityDbContext.Users.RemoveRange(identityDbContext.Users);
                await identityDbContext.SaveChangesAsync();
                var registerModel = new SignUpViewModel { Password = "haiyolo", UserName = "test" };
                HttpResponseMessage responseMessage = await this.client.PostAsync(
                    "/api/user",
                    new StringContent(JsonConvert.SerializeObject(registerModel), Encoding.UTF8, "application/json"));

                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

                User user = identityDbContext.Users.SingleOrDefault(u => u.UserName == "test");
                Assert.NotNull(user);
            }
        }

        /// <summary>
        /// should be able to login.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginShouldWork()
        {
            var user = new User { UserName = "test" };

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                identityDbContext.Users.RemoveRange(identityDbContext.Users);
                await identityDbContext.SaveChangesAsync();
                var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
                await userManager.CreateAsync(user, "pass");
            }

            var loginModel = new LoginViewModel { UserName = "test", Password = "pass" };

            HttpResponseMessage responseMessage = await this.client.PostAsync(
                "/api/user/login",
                new StringContent(JsonConvert.SerializeObject(loginModel), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            var token = JObject.Parse(await responseMessage.Content.ReadAsStringAsync())
                .GetValue("token")
                .Value<string>();
            Assert.NotNull(token);
        }
    }
}