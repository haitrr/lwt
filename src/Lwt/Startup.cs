using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Lwt.Interfaces.Services;
using Lwt.Services;
using Lwt.Interfaces;
using Lwt.Mappers;
using Lwt.Middleware;
using Lwt.Transactions;
using Lwt.Utilities;
using LWT.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Lwt
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LwtDbContext>(options => options.UseInMemoryDatabase("Lwt"));
            services.AddIdentity<User, Role>().AddEntityFrameworkStores<LwtDbContext>().AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".Lwt";

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
            services.AddMvc();

            // automapper
            services.AddAutoMapper();

            // mapper
            services.AddTransient<IMapper<TextEditModel, Text>, TextEditMapper>();

            // user
            services.AddScoped<IUserService, UserService>();
            // text
            services.AddScoped<ITextService, TextService>();
            services.AddScoped<ITextRepository, TextRepository>();

            // transaction
            services.AddScoped<ITransaction, Transaction<LwtDbContext>>();

            // utilities
            services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();

            // middleware
            services.AddSingleton<ExceptionHandleMiddleware>();

            // swagger
            services.AddSwaggerGen(
                configure => { configure.SwaggerDoc("v1", new Info() {Title = "Lwt API", Version = "v1"}); }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMiddleware<ExceptionHandleMiddleware>();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lwt API V1"); });
        }
    }
}