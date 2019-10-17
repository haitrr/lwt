namespace Lwt.Test
{
    using System.Linq;
    using Lwt.DbContexts;
    using Lwt.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Mongo2Go;

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
            builder.ConfigureServices(
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
                        MongoDbRunner runner = MongoDbRunner.Start();
                        services.AddSingleton(
                            new AppSettings()
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