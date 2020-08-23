#pragma warning disable
namespace Lwt
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Text;
    using FluentValidation.AspNetCore;
    using Lwt.Creators;
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
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;

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
        /// This method gets called by the runtime.
        /// Use this method to add services to the container.
        /// </summary>
        /// <param name="services">services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson()
                .AddFluentValidation(config => 
                    config.RegisterValidatorsFromAssemblyContaining(typeof(Startup)));

            services.AddDbContext<IdentityDbContext>(options => 
                options.UseSqlite("Data Source=lwt.db"));
            services.AddTransient<LwtDbContext>();

            // identity
            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            IConfigurationSection appSettingsSection = this.Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            services.AddSingleton(appSettings);

            byte[] key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(
                options =>
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

            services.Configure<IdentityOptions>(
                options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireNonAlphanumeric = false;
                });

            // mapper
            RegisterMappers(services);

            // user
            services.AddScoped<IUserService, UserService>();

            // cors
            services.AddCors();

            // text
            services.AddScoped<ITextService, TextService>();
            services.AddScoped<ITermService, TermService>();

            // repos
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserSettingRepository, UserSettingRepository>();
            services.AddScoped<ISqlTermRepository, SqlTermRepository>();
            services.AddScoped<ISqlTextRepository, SqlTextRepository>();
            services.AddScoped<ITextTermRepository, TextTermRepository>();
            services.AddTransient<ITextTermProcessor, TextTermProcessor>();
            services.AddTransient<ISqlUserSettingRepository, SqlUserSettingRepository>();

            // transaction
            services.AddScoped<IDbTransaction, DbTransaction<IdentityDbContext>>();

            // database seeder
            services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();
            services.AddTransient<IIndexCreator, MongoDbIndexCreator>();

            // utilities
            services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
            services.AddScoped<ILanguageHelper, LanguageHelper>();
            services.AddTransient<ITokenProvider, TokenProvider>();
            services.AddScoped<ITextSeparator, TextSeparator>();
            services.AddScoped<ITermCounter, TermCounter>();
            services.AddScoped<ITextNormalizer, TextNormalizer>();
            services.AddScoped<ISkippedWordRemover, SkippedWordRemover>();
            services.AddScoped<IUserTextGetter, UserTextGetter>();
            services.AddScoped<IUserPasswordChanger, UserPasswordChanger>();

            // middleware
            services.AddSingleton<ExceptionHandleMiddleware>();

            // add compression service
            services.AddResponseCompression();
            services.Configure<GzipCompressionProviderOptions>(
                options => { options.Level = CompressionLevel.Optimal; });

            // swagger
            RegisterSwaggerGen(services);

            // creators
            RegisterCreators(services);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">app.</param>
        /// <param name="databaseSeeder"> the database seeder.</param>
        /// <param name="indexCreator">the database indexes creator.</param>
#pragma warning disable CA1822
        public void Configure(
            IApplicationBuilder app,
            IDatabaseSeeder databaseSeeder,
            IIndexCreator indexCreator,
            IWebHostEnvironment env)
#pragma warning disable CA1822
        {
            Console.WriteLine(new { Code = JsonConvert.SerializeObject(LanguageCode.CHINESE) });
            // compress response
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseAuthentication();
            app.UseMiddleware<ExceptionHandleMiddleware>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(b => { b.MapControllers(); });
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lwt API V1"); });
            }
            indexCreator.CreateIndexesAsync()
                .GetAwaiter()
                .GetResult();
            Console.WriteLine("seeding db");
            databaseSeeder.SeedData()
                .GetAwaiter()
                .GetResult();
        }

        private static void RegisterSwaggerGen(IServiceCollection services)
        {
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lwt API", Version = "v1", });

                    var scheme = new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                    };
                    c.AddSecurityDefinition("Bearer", scheme);

                    c.AddSecurityRequirement(
                        new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme, Id = "Bearer",
                                    },
                                    Scheme = "oauth2",
                                    Name = "Bearer",
                                    In = ParameterLocation.Header,
                                },
                                new List<string>()
                            },
                        });
                });
        }

        private static void RegisterCreators(IServiceCollection services)
        {
            services.AddScoped<ITextCreator, TextCreator>();
        }

        private static void RegisterMappers(IServiceCollection services)
        {
            services.AddTransient<IMapper<TextEditModel, Text>, TextEditMapper>();
            services.AddTransient<IMapper<TextCreateModel, int, Text>, TextCreateMapper>();
            services.AddTransient<IMapper<Text, TextViewModel>, TextViewMapper>();
            services.AddTransient<IMapper<Text, TextEditDetailModel>, TextEditDetailMapper>();
            services.AddTransient<IMapper<ILanguage, LanguageViewModel>, LanguageViewMapper>();
            services.AddTransient<IMapper<TermEditModel, Term>, TermEditMapper>();
            services.AddTransient<IMapper<TermCreateModel, int, Term>, TermCreateMapper>();
            services.AddTransient<IMapper<Term, TermViewModel>, TermViewMapper>();
            services.AddTransient<IJapaneseTextSplitter, JapaneseTextSplitter>();
            services.AddSingleton<IChineseTextSplitter, ChineseTextSplitter>();
            services.AddSingleton<IMapper<User, UserView>, UserViewMapper>();
            services.AddSingleton<IMapper<UserSetting, UserSettingView>, UserSettingViewMapper>();
            services.AddSingleton<IMapper<UserSettingUpdate, UserSetting>, UserSettingUpdateMapper>();
            services.AddTransient<IMapper<Text, TextReadModel>, TextReadMapper>();
            services.AddTransient<IMapper<Term, TermMeaningDto>, TermMeaningMapper>();
        }
    }
}