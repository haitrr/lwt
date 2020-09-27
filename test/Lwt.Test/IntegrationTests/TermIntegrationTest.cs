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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// integration testing term api.
    /// </summary>
    public sealed class TermIntegrationTest : IDisposable
    {
        private readonly HttpClient client;
        private readonly LwtTestWebApplicationFactory factory;
        private readonly ITokenProvider tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermIntegrationTest"/> class.
        /// </summary>
        public TermIntegrationTest()
        {
            this.factory = new LwtTestWebApplicationFactory();
            this.tokenProvider = this.factory.Services.GetService<ITokenProvider>();
            this.client = this.factory.CreateClient();
        }

        /// <summary>
        /// test create term.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateTermAsyncShouldCreateTerm()
        {
            var user = new User { UserName = "test" };
            TestDbHelper.CleanTable<Term>(this.factory.Services);

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<IdentityDbContext>();
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }

            var termCreateModel = new TermCreateModel
            {
                LanguageCode = LanguageCode.ENGLISH,
                Content = "test",
                LearningLevel = LearningLevel.Learning1,
                Meaning = "yolo",
            };

            string token = this.tokenProvider.GenerateUserToken(user);
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage? result = await this.client.PostAsync(
                "api/term",
                new StringContent(JsonConvert.SerializeObject(termCreateModel), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var id = (int)JsonConvert.DeserializeObject<dynamic>(await result.Content.ReadAsStringAsync())
                .id;

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                List<Term> terms = await identityDbContext.Set<Term>()
                    .Where(t => t.Id == id)
                    .ToListAsync();
                Term? term = Assert.Single(terms);
                Assert.NotNull(term);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal(user.Id, term.UserId);
                Assert.Equal(termCreateModel.LanguageCode, term.LanguageCode);
                Assert.Equal("TEST", term.Content);
                Assert.Equal(termCreateModel.Meaning, term.Meaning);
                Assert.Equal(termCreateModel.LearningLevel, term.LearningLevel);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        /// <summary>
        /// should able to edit term.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditTermShouldWork()
        {
            var user = new User { UserName = "test" };

            var existingTerm = new Term
            {
                LanguageCode = LanguageCode.CHINESE, LearningLevel = LearningLevel.Learning2,
            };

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<IdentityDbContext>();

                foreach (Term t in dbContext.Set<Term>())
                {
                    dbContext.Set<Term>()
                        .Remove(t);
                }

                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                existingTerm.UserId = user.Id;
                dbContext.Set<Term>()
                    .Add(existingTerm);
                dbContext.SaveChanges();
            }

            var termEditModel = new TermEditModel { LearningLevel = LearningLevel.Learning1, Meaning = "yolo", };

            string token = this.tokenProvider.GenerateUserToken(user);
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await this.client.PutAsync(
                $"api/term/{existingTerm.Id}",
                new StringContent(JsonConvert.SerializeObject(termEditModel), Encoding.UTF8, "application/json"));

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<IdentityDbContext>();
                List<Term> terms = await dbContext.Set<Term>()
                    .Where(_ => true)
                    .ToListAsync();
                Term? term = Assert.Single(terms);
                Assert.NotNull(term);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal(termEditModel.Meaning, term.Meaning);
                Assert.Equal(termEditModel.LearningLevel, term.LearningLevel);
            }
        }

        /// <summary>
        /// should return 401 if not logged in.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetByIdAsyncShouldReturn401IfNotLoggedIn()
        {
            var expectedResponse = HttpStatusCode.Unauthorized;

            HttpResponseMessage response = await this.client.GetAsync($"api/term/{1.ToString()}");

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

            HttpResponseMessage response = await this.client.GetAsync("api/term/{1}");

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

            var term = new Term
            {
                UserId = user.Id,
                LanguageCode = LanguageCode.ENGLISH,
                Content = "test",
                LearningLevel = LearningLevel.Unknown,
            };

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<IdentityDbContext>();
                dbContext.Users.Add(user);
                term.UserId = user.Id;
                dbContext.Set<Term>()
                    .Add(term);
                dbContext.SaveChanges();
            }

            string token = this.tokenProvider.GenerateUserToken(user);
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var expectedResponse = HttpStatusCode.OK;

            HttpResponseMessage response = await this.client.GetAsync($"api/term/{term.Id}");

            Assert.Equal(expectedResponse, response.StatusCode);
            var termViewModel = JsonConvert.DeserializeObject<TermViewModel>(
                await response.Content.ReadAsStringAsync());
        }
    }
}