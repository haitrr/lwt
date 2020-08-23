namespace Lwt.Test.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.ViewModels.User;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// test user apis integration.
    /// </summary>
    public class UserIntegrationTest : IDisposable
    {
        private readonly HttpClient client;
        private readonly LwtTestWebApplicationFactory factory;
        private readonly ITokenProvider tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIntegrationTest"/> class.
        /// </summary>
        public UserIntegrationTest()
        {
            this.factory = new LwtTestWebApplicationFactory();
            this.tokenProvider = this.factory.Services.GetService<ITokenProvider>();
            this.client = this.factory.CreateClient();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UserIntegrationTest"/> class.
        /// </summary>
        ~UserIntegrationTest()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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

        /// <summary>
        /// should be able to get user profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldAbleToGetProfile()
        {
            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                var user = new User() { UserName = "test", Email = "test@yopmail.com" };
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                identityDbContext.Users.RemoveRange(identityDbContext.Users);
                await identityDbContext.SaveChangesAsync();
                identityDbContext.Users.Add(user);
                await identityDbContext.SaveChangesAsync();
                string token = this.tokenProvider.GenerateUserToken(user);
                this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                HttpResponseMessage response = await this.client.GetAsync("/api/user");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                JToken data = JToken.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(user.UserName, data.Value<string>("userName"));
                Assert.Equal(user.Email, data.Value<string>("email"));
            }
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

                User? user = identityDbContext.Users.SingleOrDefault(u => u.UserName == "test");
                Assert.NotNull(user);
            }
        }

        /// <summary>
        /// should be able to get setting of the user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToGetSetting()
        {
            var user = new User() { UserName = "test" };

            using (WebApplicationFactory<Startup> autheticatedFactory = this.factory.ApplyFakeUser(user))
            using (HttpClient authenticatedClient = autheticatedFactory.CreateClient())
            using (IServiceScope scope = autheticatedFactory.Services.CreateScope())
            {
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                await identityDbContext.Users.AddAsync(user);
                await identityDbContext.SaveChangesAsync();
                var userSetting = new UserSetting()
                {
                    UserId = user.Id,
                    LanguageSettings = new List<LanguageSetting>()
                    {
                        new LanguageSetting { DictionaryLanguage = "vi", LanguageCode = LanguageCode.ENGLISH },
                    },
                };

                using (IdentityDbContext dc = TestDbHelper.GetDbContext(this.factory))
                {
                    dc.Set<UserSetting>()
                        .Add(userSetting);
                    dc.SaveChanges();
                }

                HttpResponseMessage responseMessage = await authenticatedClient.GetAsync("/api/user/setting");

                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
                var content =
                    JsonConvert.DeserializeObject<UserSettingView>(await responseMessage.Content.ReadAsStringAsync());
                Assert.Equal(content.UserId, user.Id);
            }
        }

        /// <summary>
        /// should not be able to get setting of the user if not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotBeAbleToGetSettingIfNotExist()
        {
            var user = new User() { UserName = "test", Id = 1 };

            using (WebApplicationFactory<Startup> autheticatedFactory = this.factory.ApplyFakeUser(user))
            using (HttpClient authenticatedClient = autheticatedFactory.CreateClient())
            using (IServiceScope scope = autheticatedFactory.Services.CreateScope())
            {
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                await identityDbContext.Users.AddAsync(user);
                await identityDbContext.SaveChangesAsync();
                HttpResponseMessage responseMessage = await authenticatedClient.GetAsync("/api/user/setting");

                Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
            }
        }

        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.client.Dispose();
                this.factory.Dispose();
            }
        }
    }
}