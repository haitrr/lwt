namespace Lwt.Utilities
{
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IUserRepository userRepository;

        private readonly LwtDbContext lwtDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="userRepository"> user repo.</param>
        /// <param name="lwtDbContext">the db context.</param>
        public DatabaseSeeder(IUserRepository userRepository, LwtDbContext lwtDbContext)
        {
            this.userRepository = userRepository;
            this.lwtDbContext = lwtDbContext;
        }

        /// <inheritdoc />
        public void SeedData()
        {
            this.lwtDbContext.Database.EnsureCreated();
            var hai = new User { UserName = "hai" };
            this.userRepository.CreateAsync(hai, "q");
        }
    }
}