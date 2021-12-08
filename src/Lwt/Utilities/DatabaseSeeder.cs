namespace Lwt.Utilities;

using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly IUserRepository userRepository;

    private readonly IdentityDbContext lwtDbContext;
    private readonly ISqlTextRepository textRepository;
    private readonly ILogger<DatabaseSeeder> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
    /// </summary>
    /// <param name="userRepository"> user repo.</param>
    /// <param name="lwtDbContext">the db context.</param>
    /// <param name="textRepository">the text repository.</param>
    /// <param name="logger">the logger.</param>
    public DatabaseSeeder(
        IUserRepository userRepository,
        IdentityDbContext lwtDbContext,
        ISqlTextRepository textRepository,
        ILogger<DatabaseSeeder> logger)
    {
        this.userRepository = userRepository;
        this.lwtDbContext = lwtDbContext;
        this.textRepository = textRepository;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task SeedData()
    {
        this.lwtDbContext.Database.Migrate();

        User? hai = await this.userRepository.GetByUserNameAsync("hai");

        if (hai != null)
        {
            this.logger.LogInformation("Identity has already seeded");
        }
        else
        {
            this.logger.LogInformation("Seeding user");
            hai = new User {Id = 1, UserName = "hai"};
            await this.userRepository.CreateAsync(hai, "q");
        }

        if (await this.textRepository.CountAsync() > 0)
        {
            this.logger.LogInformation("Database has already seeded");
        }
    }
}