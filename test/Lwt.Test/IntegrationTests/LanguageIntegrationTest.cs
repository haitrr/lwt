namespace Lwt.Test.IntegrationTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Lwt.Models;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// language integration test.
    /// </summary>
    public class LanguageIntegrationTest
    {
        /// <summary>
        /// should be able to get languages.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBeAbleToGetLanguages()
        {
            using (var factory = new LwtTestWebApplicationFactory())
            using (HttpClient client = factory.CreateDefaultClient())
            using (HttpResponseMessage responseMessage = await client.GetAsync("api/language"))
            {
                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
                var languages =
                    JsonConvert.DeserializeObject<ICollection<LanguageViewModel>>(
                        await responseMessage.Content.ReadAsStringAsync());
                Assert.Contains(languages, language => language.Code == "en");
                Assert.Contains(languages, language => language.Code == "zh");
                Assert.Contains(languages, language => language.Code == "ja");
                Assert.Contains(languages, language => language.Code == "vi");
            }
        }
    }
}