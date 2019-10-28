namespace Lwt.Test.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Models;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Mongo2Go;
    using MongoDB.Driver;
    using Xunit;

    /// <summary>
    /// test database setup.
    /// </summary>
    public class DatabaseSeedingTest
    {
        /// <summary>
        /// database seeding should work.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DatabaseSeedingShouldWork()
        {
            using (var factory = new WebApplicationFactory<Startup>())
            using (MongoDbRunner mongoDbRunner = MongoDbRunner.Start())
            using (WebApplicationFactory<Startup> seedEnabledFactory = factory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            // Remove the app's ApplicationDbContext registration.
                            ServiceDescriptor descriptor = services.SingleOrDefault(
                                d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));

                            if (descriptor != null)
                            {
                                services.Remove(descriptor);
                            }

                            // Add ApplicationDbContext using an in-memory database for testing.
                            services.AddDbContext<IdentityDbContext>(
                                (options, context) => { context.UseInMemoryDatabase("InMemoryDbForTesting"); });

                            ServiceDescriptor appSettingDescriptor = services.SingleOrDefault(
                                d => d.ServiceType == typeof(AppSettings));

                            if (appSettingDescriptor != null)
                            {
                                var appSetting = (AppSettings)appSettingDescriptor.ImplementationInstance;
                                services.Remove(appSettingDescriptor);
                                services.AddSingleton(
                                    new AppSettings
                                    {
                                        MongoDatabase = appSetting.MongoDatabase,
                                        Secret = appSetting.Secret,

                                        // this one will dispose before the runner dispose.
                                        // ReSharper disable once AccessToDisposedClosure
                                        MongoConnectionString = mongoDbRunner.ConnectionString,
                                    });
                            }
                        });
                }))
            {
                var dbContext = seedEnabledFactory.Services.GetRequiredService<LwtDbContext>();
                Assert.Equal(
                    1,
                    await dbContext.GetCollection<Text>().Find(_ => true).CountDocumentsAsync());

                using (IServiceScope scope = seedEnabledFactory.Services.CreateScope())
                using (var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>())
                {
                    User hai = identityDbContext.Users.SingleOrDefault(u => u.UserName == "hai");
                    Assert.NotNull(hai);
                }
            }
        }
    }
}