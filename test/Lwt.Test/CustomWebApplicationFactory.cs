namespace Lwt.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Mongo2Go;
    using Moq;

    /// <summary>
    /// custom web application factory for lwt for testing.
    /// </summary>
    /// <typeparam name="TStartup">the startup.</typeparam>
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(
                services =>
                {
                    // Replace database seeder.
                    ServiceDescriptor databaseSeederDescriptor =
                        services.Single(s => s.ServiceType == typeof(IDatabaseSeeder));

                    services.Remove(databaseSeederDescriptor);

                    var mock = new Mock<IDatabaseSeeder>();
                    mock.Setup(s => s.SeedData())
                        .Returns(Task.CompletedTask);
                    services.AddTransient(resolver => mock.Object);

                    // Replace database seeder.
                    ServiceDescriptor indexCreatorDescriptor =
                        services.Single(s => s.ServiceType == typeof(IIndexCreator));

                    services.Remove(indexCreatorDescriptor);

                    var indexCreatorMock = new Mock<IIndexCreator>();
                    indexCreatorMock.Setup(s => s.CreateIndexesAsync())
                        .Returns(Task.CompletedTask);
                    services.AddTransient(resolver => indexCreatorMock.Object);

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
                        MongoDbRunner runner = MongoDbRunner.Start();
                        services.AddSingleton(runner);
                        services.AddSingleton(
                            new AppSettings
                            {
                                MongoDatabase = appSetting.MongoDatabase,
                                Secret = appSetting.Secret,
                                MongoConnectionString = runner.ConnectionString,
                            });
                    }
                });
        }
    }
}