namespace Lwt.Test.IntegrationTests;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly ITokenProvider tokenProvider;
    private readonly User user;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextIntegrationTest"/> class.
    /// </summary>
    public TextIntegrationTest()
    {
        this.factory = new LwtTestWebApplicationFactory();
        this.tokenProvider = this.factory.Services.GetRequiredService<ITokenProvider>();
        this.user = new User() {UserName = "test"};

        using (IServiceScope scope = this.factory.Services.CreateScope())
        {
            var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
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
        TestDbHelper.CleanTable<Text>(this.factory.Services);
        var body = new {title = "test text", content = "this is a test text", languageCode = "en"};
        string content = JsonConvert.SerializeObject(body);
        HttpResponseMessage responseMessage = await this.client.PostAsync(
            "api/text",
            new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        var id = (int) JsonConvert.DeserializeObject<dynamic>(await responseMessage.Content.ReadAsStringAsync()) !
            .id;

        using (IServiceScope scope = this.factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            Text text = await dbContext.Set<Text>()
                .SingleAsync(t => t.Id == id);

            Assert.Equal(body.title, text.Title);
            Assert.Equal(body.content, text.Content);
            Assert.Equal(body.languageCode, text.LanguageCode.ToString());
            Assert.Equal(this.user.Id, text.UserId);
        }
    }

    /// <summary>
    /// should be able test get text list.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ShouldAbleToGetListOfTexts()
    {
        TestDbHelper.CleanTable<Text>(this.factory.Services);

        using (IServiceScope? scope = this.factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            for (var i = 0; i < 20; i++)
            {
                dbContext.Set<Text>()
                    .Add(
                        new Text()
                        {
                            Title = "test",
                            Content = "test",
                            LanguageCode = LanguageCode.ENGLISH,
                            UserId = this.user.Id,
                        });
            }

            dbContext.SaveChanges();
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
            Assert.Equal(LanguageCode.ENGLISH, LanguageCode.GetFromString(item.Value<string>("languageCode")));
        }
    }

    /// <summary>
    /// should be able to delete my text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ShouldBeAbleToDeleteText()
    {
        TestDbHelper.CleanTable<Text>(this.factory.Services);
        var text = new Text
        {
            Title = "test", Content = "test", LanguageCode = LanguageCode.ENGLISH, UserId = this.user.Id,
        };

        using (IServiceScope? scope = this.factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
            dbContext.Set<Text>()
                .Add(text);
            dbContext.SaveChanges();
        }

        HttpResponseMessage responseMessage = await this.client.DeleteAsync($"api/text/{text.Id}");
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

        using (IServiceScope? scope = this.factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
            Text deletedText = await dbContext.Set<Text>()
                .SingleOrDefaultAsync();
            Assert.Null(deletedText);
        }
    }

    /// <summary>
    /// should be able to edit my text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ShouldBeAbleToEditText()
    {
        TestDbHelper.CleanTable<Text>(this.factory.Services);
        var text = new Text
        {
            Title = "test",
            Content = "this is a test text",
            LanguageCode = LanguageCode.ENGLISH,
            UserId = this.user.Id,
        };

        using (IdentityDbContext dbContext = TestDbHelper.GetDbContext(this.factory))
        {
            dbContext.Set<Text>()
                .Add(text);
            dbContext.SaveChanges();
        }

        var editContent = new
        {
            languageCode = LanguageCode.JAPANESE, title = "test edited", content = "edited content",
        };

        HttpResponseMessage responseMessage = await this.client.PutAsync(
            $"api/text/{text.Id}",
            new StringContent(
                JsonConvert.SerializeObject(editContent),
                Encoding.UTF8,
                MediaTypeNames.Application.Json));
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

        using (IdentityDbContext dc = TestDbHelper.GetDbContext(this.factory))
        {
            Text editedText = await dc.Set<Text>()
                .Where(t => t.Id == text.Id)
                .SingleAsync();
            Assert.Equal(editContent.title, editedText.Title);
            Assert.Equal(editContent.languageCode, editedText.LanguageCode);
            Assert.Equal(editContent.content, editedText.Content);
        }
    }

    /// <summary>
    /// should be able to read my text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ShouldBeAbleToGetEditDetails()
    {
        TestDbHelper.CleanTable<Text>(this.factory.Services);
        var text = new Text
        {
            Title = "test",
            Content = "this is a test text",
            LanguageCode = LanguageCode.ENGLISH,
            UserId = this.user.Id,
        };

        using (IdentityDbContext dc = TestDbHelper.GetDbContext(this.factory))
        {
            dc.Set<Text>()
                .Add(text);
            dc.SaveChanges();
        }

        HttpResponseMessage responseMessage = await this.client.GetAsync($"api/text/{text.Id}/edit-details");
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        JToken content = JToken.Parse(await responseMessage.Content.ReadAsStringAsync());
        Assert.Equal(text.Title, content.Value<string>("title"));
        Assert.Equal(
            text.Id.ToString()
                .ToLower(),
            content.Value<string>("id")
                .ToLower());
        Assert.Equal(text.LanguageCode, LanguageCode.GetFromString(content.Value<string>("languageCode")));
        Assert.Equal(text.Content, content.Value<string>("content"));
    }
}