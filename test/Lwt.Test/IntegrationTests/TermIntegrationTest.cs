namespace Lwt.Test.IntegrationTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// integration testing term api.
    /// </summary>
    #pragma warning disable CA1001
    public class TermIntegrationTest
    #pragma warning restore CA1001
    {
        private readonly HttpClient client;
        private readonly CustomWebApplicationFactory<Startup> factory;
        private readonly LwtDbContext lwtDbContext;
        private readonly ITokenProvider tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermIntegrationTest"/> class.
        /// </summary>
        public TermIntegrationTest()
        {
            this.factory = new CustomWebApplicationFactory<Startup>();
            this.tokenProvider = this.factory.Services.GetService<ITokenProvider>();
            this.lwtDbContext = this.factory.Services.GetService<LwtDbContext>();
            this.client = this.factory.CreateClient();
        }

        /// <summary>
        /// should return 401 if not logged in.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetByIdAsyncShouldReturn401IfNotLoggedIn()
        {
            var expectedResponse = HttpStatusCode.Unauthorized;

            HttpResponseMessage response = await this.client.GetAsync($"api/term/{Guid.NewGuid().ToString()}");

            Assert.Equal(expectedResponse, response.StatusCode);
        }

        /// <summary>
        /// should return 401 if not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetByIdAsyncShouldReturn404IfNotFound()
        {
            var user = new User { UserName = "test" };

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<IdentityDbContext>();
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }

            string token = this.tokenProvider.GenerateUserToken(user);
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var expectedResponse = HttpStatusCode.NotFound;

            HttpResponseMessage response = await this.client.GetAsync($"api/term/{Guid.NewGuid().ToString()}");

            Assert.Equal(expectedResponse, response.StatusCode);
        }

        /// <summary>
        /// should return text if found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetByIdAsyncShouldReturnTextIfFound()
        {
            var user = new User { UserName = "test" };

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<IdentityDbContext>();
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }

            var term = new Term
            {
                CreatorId = user.Id,
                Language = Language.English,
                Content = "test",
            };
            this.lwtDbContext.GetCollection<Term>()
                .InsertOne(term);

            string token = this.tokenProvider.GenerateUserToken(user);
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var expectedResponse = HttpStatusCode.OK;

            HttpResponseMessage response = await this.client.GetAsync($"api/term/{term.Id.ToString()}");

            Assert.Equal(expectedResponse, response.StatusCode);
            var termViewModel = JsonConvert.DeserializeObject<TermViewModel>(
                await response.Content.ReadAsStringAsync());
            Assert.Equal(termViewModel.Content, term.Content);
            Assert.Equal(termViewModel.Id, term.Id);
        }
    }
}