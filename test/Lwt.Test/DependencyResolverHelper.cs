namespace Lwt.Test
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// dependency resolver helper.
    /// </summary>
    public class DependencyResolverHelper
    {
        private readonly IWebHost webHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolverHelper"/> class.
        /// </summary>
        public DependencyResolverHelper()
        {
            this.webHost = WebHost.CreateDefaultBuilder().ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile(
                    Path.Combine(
                        hostingContext.HostingEnvironment.ContentRootPath,
                        "../../../../../src/Lwt/appsettings.json"),
                    false);
            }).UseStartup<Startup>().Build();
        }

        /// <summary>
        /// get a service from container.
        /// </summary>
        /// <typeparam name="T">type of service.</typeparam>
        /// <returns> the service.</returns>
        public T GetService<T>()
        {
            using (IServiceScope serviceScope = this.webHost.Services.CreateScope())
            {
                IServiceProvider services = serviceScope.ServiceProvider;

                var scopedService = services.GetRequiredService<T>();
                return scopedService;
            }
        }
    }
}