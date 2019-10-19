namespace Lwt.Test.IntegrationTests
{
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
    using Xunit;

    /// <summary>
    /// test text apis integration.
    /// </summary>
    public class TextIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient client;
        private readonly ITokenProvider tokenProvider;
        private readonly LwtDbContext lwtDbContext;
        private readonly CustomWebApplicationFactory<Startup> factory;
        private readonly User user;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextIntegrationTest"/> class.
        /// </summary>
        /// <param name="factory">the web host factory.</param>
        public TextIntegrationTest(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
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

        /// <summary>
        /// should be able to create text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldAbleToCreateText()
        {
            await this.lwtDbContext.GetCollection<Text>()
                .FindOneAndDeleteAsync(_ => true);
            var body = new { title = "test text", content = "this is a test text", language = 1 };
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
            Assert.Equal(body.language, (int)text.Language);
            Assert.Equal(this.user.Id, text.CreatorId);
        }
    }
}
