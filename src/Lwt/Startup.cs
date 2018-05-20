using Lwt.DbContexts;
using Lwt.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Lwt.Interfaces.Services;
using Lwt.Services;

namespace Lwt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LwtDbContext>(options => options.UseInMemoryDatabase("Lwt"));
            services.AddIdentity<User, Role>().AddEntityFrameworkStores<LwtDbContext>().AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options => options.Cookie.Name = ".Lwt");
            services.AddMvc();

            // automapper
            ServiceCollectionExtensions.UseStaticRegistration = false;
            services.AddAutoMapper();

            // user
            services.AddScoped<IUserService, UserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
