namespace Lwt.Test.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// test text apis integration.
    /// </summary>
    public sealed class TextIntegrationTest : IDisposable
    {
        private readonly HttpClient client;
        private readonly LwtTestWebApplicationFactory factory;
        private readonly LwtDbContext lwtDbContext;
        private readonly ITokenProvider tokenProvider;
        private readonly User user;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextIntegrationTest"/> class.
        /// </summary>
        public TextIntegrationTest()
        {
            this.factory = new LwtTestWebApplicationFactory();
            this.tokenProvider = this.factory.Services.GetService<ITokenProvider>();
            this.lwtDbContext = this.factory.Services.GetService<LwtDbContext>();
            this.user = new User() { UserName = "test" };

            using (IServiceScope scope = this.factory.Services.CreateScope())
            {
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                identityDbContext.Users.Add(this.user);
                identityDbContext.SaveChangesAsync();
            }

            string token = this.tokenProvider.GenerateUserToken(this.user);
            this.client = this.factory.CreateClient();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        /// <summary>
        /// should be able to create text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldAbleToCreateText()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);
            var body = new { title = "test text", content = "this is a test text", languageCode = "en" };
            string content = JsonConvert.SerializeObject(body);
            HttpResponseMessage responseMessage = await this.client.PostAsync(
                "api/text",
                new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Text text = await this.lwtDbContext.GetCollection<Text>()
                .Find(_ => true)
                .SingleOrDefaultAsync();
            Assert.NotNull(text);
            Assert.Equal(body.title, text.Title);
            Assert.Equal(body.content, text.Content);
            Assert.Equal(body.languageCode, text.LanguageCode.ToString());
            Assert.Equal(this.user.Id, text.CreatorId);
        }

        /// <summary>
        /// should be able test get text list.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldAbleToGetListOfTexts()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);

            for (var i = 0; i < 20; i++)
            {
                await this.lwtDbContext.GetCollection<Text>()
                    .InsertOneAsync(
                        new Text()
                        {
                            Title = "test", Content = "test", LanguageCode = LanguageCode.ENGLISH, CreatorId = this.user.Id,
                        });
            }

            HttpResponseMessage responseMessage = await this.client.GetAsync("api/text?page=1&itemPerPage=7");
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            JToken rs = JToken.Parse(await responseMessage.Content.ReadAsStringAsync());
            var data = rs.Value<JArray>("items");
            Assert.Equal(20, rs.Value<int>("total"));
            Assert.Equal(7, data.Count);

            foreach (JToken item in data)
            {
                Assert.Equal("test", item.Value<string>("title"));
                Assert.Equal(LanguageCode.ENGLISH, item.Value<LanguageCode>("languageCode"));
                Assert.NotNull(item.SelectToken("counts"));
            }
        }

        /// <summary>
        /// should be able to delete my text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToDeleteText()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);
            var text = new Text
            {
                Title = "test", Content = "test", LanguageCode = LanguageCode.ENGLISH, CreatorId = this.user.Id,
            };
            await this.lwtDbContext.GetCollection<Text>()
                .InsertOneAsync(text);

            HttpResponseMessage responseMessage = await this.client.DeleteAsync($"api/text/{text.Id}");
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Text deletedText = await this.lwtDbContext.GetCollection<Text>()
                .Find(_ => true)
                .SingleOrDefaultAsync();
            Assert.Null(deletedText);
        }

        /// <summary>
        /// should be able to edit my text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToEditText()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);
            var text = new Text
            {
                Title = "test",
                Content = "this is a test text",
                LanguageCode = LanguageCode.ENGLISH,
                CreatorId = this.user.Id,
            };
            await this.lwtDbContext.GetCollection<Text>()
                .InsertOneAsync(text);

            var editContent = new { languageCode = LanguageCode.JAPANESE, title = "test edited", content = "edited content" };

            HttpResponseMessage responseMessage = await this.client.PutAsync(
                $"api/text/{text.Id}",
                new StringContent(
                    JsonConvert.SerializeObject(editContent),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Text editedText = await this.lwtDbContext.GetCollection<Text>()
                .Find(t => t.Id == text.Id)
                .SingleAsync();
            Assert.Equal(editContent.title, editedText.Title);
            Assert.Equal(editContent.languageCode, editedText.LanguageCode);
            Assert.Equal(editContent.content, editedText.Content);
        }

        /// <summary>
        /// should be able to read my text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToGetEditDetails()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);
            var text = new Text
            {
                Title = "test",
                Content = "this is a test text",
                LanguageCode = LanguageCode.ENGLISH,
                CreatorId = this.user.Id,
            };
            await this.lwtDbContext.GetCollection<Text>()
                .InsertOneAsync(text);

            HttpResponseMessage responseMessage = await this.client.GetAsync($"api/text/{text.Id}/edit-details");
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            JToken content = JToken.Parse(await responseMessage.Content.ReadAsStringAsync());
            Assert.Equal(text.Title, content.Value<string>("title"));
            Assert.Equal(
                text.Id.ToString()
                    .ToLower(),
                content.Value<string>("id")
                    .ToLower());
            Assert.Equal(text.LanguageCode, content.Value<LanguageCode>("languageCode"));
            Assert.Equal(text.Content, content.Value<string>("content"));
        }

        /// <summary>
        /// should be able to read my text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToReadText()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);
            var text = new Text
            {
                Title = "test",
                Content = "this is a test text",
                LanguageCode = LanguageCode.ENGLISH,
                CreatorId = this.user.Id,
            };
            await this.lwtDbContext.GetCollection<Text>()
                .InsertOneAsync(text);

            HttpResponseMessage responseMessage = await this.client.GetAsync($"api/text/{text.Id}");
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            JToken content = JToken.Parse(await responseMessage.Content.ReadAsStringAsync());
            Assert.Equal(text.Title, content.Value<string>("title"));
            Assert.Equal(
                text.Id.ToString()
                    .ToLower(),
                content.Value<string>("id")
                    .ToLower());
            Assert.Equal(text.LanguageCode, content.Value<LanguageCode>("languageCode"));
            Assert.NotNull(content.Value<JArray>("terms"));
        }

        /// <summary>
        /// should be able to edit my text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToSetBookmark()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .DeleteManyAsync(_ => true);
            var text = new Text
            {
                Title = "test",
                Content = "this is a test text",
                LanguageCode = LanguageCode.ENGLISH,
                CreatorId = this.user.Id,
                Words = new List<string>
                {
                    "what",
                    "does",
                    "the",
                    "fox",
                    "say",
                },
            };
            await this.lwtDbContext.GetCollection<Text>()
                .InsertOneAsync(text);

            var bookMarkContent = new { termIndex = 2UL };

            HttpResponseMessage responseMessage = await this.client.PatchAsync(
                $"api/text/{text.Id}/bookmark",
                new StringContent(
                    JsonConvert.SerializeObject(bookMarkContent),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Text editedText = await this.lwtDbContext.GetCollection<Text>()
                .Find(t => t.Id == text.Id)
                .SingleAsync();
            Assert.Equal(bookMarkContent.termIndex, editedText.Bookmark);
        }
    }
}