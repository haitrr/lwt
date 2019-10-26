namespace Lwt.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Mongo2Go;
    using Moq;

    /// <summary>
    /// custom web application factory for lwt for testing.
    /// </summary>
    public class LwtTestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly MongoDbRunner mongoDbRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="LwtTestWebApplicationFactory"/> class.
        /// </summary>
        public LwtTestWebApplicationFactory()
        {
            this.mongoDbRunner = MongoDbRunner.Start();
        }

        /// <summary>
        /// return a new factory with fake user to test secured apis.
        /// </summary>
        /// <param name="user">the fake user.</param>
        /// <returns>the new factory.</returns>
        public WebApplicationFactory<Startup> ApplyFakeUser(User? user = null)
        {
            return this.WithWebHostBuilder(
                builder => builder.ConfigureTestServices(
                    services =>
                    {
                        var mockOptions = new Mock<IConfigureOptions<MvcOptions>>();
                        mockOptions.Setup(c => c.Configure(It.IsAny<MvcOptions>()))
                            .Callback<MvcOptions>(
                                options =>
                                {
                                    options.Filters.Add(new AllowAnonymousFilter());

                                    options.Filters.Add(user != null ? new FakeUserFilter(user) : new FakeUserFilter());
                                });
                        services.AddSingleton(mockOptions.Object);
                    }));
        }

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
                        services.AddSingleton(
                            new AppSettings
                            {
                                MongoDatabase = appSetting.MongoDatabase,
                                Secret = appSetting.Secret,
                                MongoConnectionString = this.mongoDbRunner.ConnectionString,
                            });
                    }
                });
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.mongoDbRunner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}