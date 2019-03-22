using System.IO;

namespace Lwt.Test
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class DependencyResolverHelpercs
    {
        private readonly IWebHost webHost;

        /// <inheritdoc />
        public DependencyResolverHelpercs()
        {
            this.webHost = WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddJsonFile(Path.Combine(hostingContext.HostingEnvironment.ContentRootPath,"../../../../../src/Lwt/appsettings.json"),optional:false);
                    })
                .UseStartup<Startup>()
                .Build();
        }

        public T GetService<T>()
        {
            using (IServiceScope serviceScope = this.webHost.Services.CreateScope())
            {
                IServiceProvider services = serviceScope.ServiceProvider;
                try
                {
                    var scopedService = services.GetRequiredService<T>();
                    return scopedService;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };
        }
    }
}