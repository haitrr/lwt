namespace Lwt
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using FluentValidation.AspNetCore;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Mappers;
    using Lwt.Middleware;
    using Lwt.Models;
    using Lwt.Repositories;
    using Lwt.Services;
    using Lwt.Transactions;
    using Lwt.Utilities;
    using Lwt.ViewModels;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Swashbuckle.AspNetCore.Swagger;

    /// <summary>
    /// statup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">the configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddFluentValidation(config => config.RegisterValidatorsFromAssemblyContaining(typeof(Startup)));

            services.AddDbContext<IdentityDbContext>(options => options.UseSqlite("Data Source=lwt.db"));
            services.AddTransient<LwtDbContext>();

            // identity
            services.AddIdentity<User, Role>().AddEntityFrameworkStores<IdentityDbContext>().AddDefaultTokenProviders();

            IConfigurationSection appSettingsSection = this.Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            byte[] key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            });

            // mapper
            services.AddTransient<IMapper<TextEditModel, Text>, TextEditMapper>();
            services.AddTransient<IMapper<TextCreateModel, Guid, Text>, TextCreateMapper>();
            services.AddTransient<IMapper<Text, TextViewModel>, TextViewMapper>();
            services.AddTransient<IMapper<ILanguage, LanguageViewModel>, LanguageViewMapper>();
            services.AddTransient<IMapper<TermEditModel, Term>, TermEditMapper>();
            services.AddTransient<IMapper<TermCreateModel, Guid, Term>, TermCreateMapper>();
            services.AddTransient<IMapper<Term, TermViewModel>, TermViewMapper>();
            services.AddTransient<ITextSplitter, TextSplitter>();

            // user
            services.AddScoped<IUserService, UserService>();

            // cors
            services.AddCors();

            // text
            services.AddScoped<ITextService, TextService>();
            services.AddScoped<ITextRepository, TextRepository>();
            services.AddScoped<ITermService, TermService>();

            // repos
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITermRepository, TermRepository>();

            // transaction
            services.AddScoped<ITransaction, Transaction<IdentityDbContext>>();

            // database seeder
            services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();

            // utilities
            services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
            services.AddScoped<ILanguageHelper, LanguageHelper>();
            services.AddScoped<ITokenProvider, TokenProvider>();

            // middleware
            services.AddSingleton<ExceptionHandleMiddleware>();

            // swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Lwt API", Version = "v1" });

                c.AddSecurityDefinition("Bearer",
                    new ApiKeyScheme()
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey",
                    });

                c.AddSecurityRequirement(
                    new Dictionary<string, IEnumerable<string>> { { "Bearer", Array.Empty<string>() }, });
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">app.</param>
        /// <param name="env">env.</param>
        /// <param name="databaseSeeder"> the database seeder.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IDatabaseSeeder databaseSeeder)
        {
            /*if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }*/
            app.UseDeveloperExceptionPage();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseAuthentication();
            app.UseMiddleware<ExceptionHandleMiddleware>();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lwt API V1"); });
            databaseSeeder.SeedData().Wait();
        }
    }
}