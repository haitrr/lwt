namespace Lwt
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Mappers;
    using Lwt.Middleware;
    using Lwt.Models;
    using LWT.Models;
    using Lwt.Services;
    using Lwt.Transactions;
    using Lwt.Utilities;
    using Lwt.ViewModels;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.Swagger;

    /// <summary>
    /// statup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LwtDbContext>(options => options.UseInMemoryDatabase("Lwt"));
            
            // identity
            services.AddIdentity<User, Role>().AddEntityFrameworkStores<LwtDbContext>().AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".Lwt";

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
            services.AddMvc().AddFluentValidation();

            // automapper
            services.AddAutoMapper();

            // mapper
            services.AddTransient<IMapper<TextEditModel, Text>, TextEditMapper>();
            services.AddTransient<BaseMapper<TextCreateModel, Guid, Text>, TextCreateMapper>();

            // user
            services.AddScoped<IUserService, UserService>();

            // cors
            services.AddCors();

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
                configure => { configure.SwaggerDoc("v1", new Info { Title = "Lwt API", Version = "v1" }); });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">app.</param>
        /// <param name="env">env.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMiddleware<ExceptionHandleMiddleware>();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lwt API V1"); });
        }
    }
}