namespace Lwt.Utilities
{
    using System;
    using System.Threading.Tasks;

    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IUserRepository userRepository;

        private readonly IdentityDbContext lwtDbContext;

        private readonly ILanguageRepository languageRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="userRepository"> user repo.</param>
        /// <param name="lwtDbContext">the db context.</param>
        /// <param name="languageRepository">the language repository.</param>
        public DatabaseSeeder(
            IUserRepository userRepository,
            IdentityDbContext lwtDbContext,
            ILanguageRepository languageRepository)
        {
            this.userRepository = userRepository;
            this.lwtDbContext = lwtDbContext;
            this.languageRepository = languageRepository;
        }

        /// <inheritdoc />
        public async Task SeedData()
        {
            bool notSeeded = this.lwtDbContext.Database.EnsureCreated();

            if (!notSeeded)
            {
                return;
            }

            User hai = await this.userRepository.GetByUserNameAsync("hai");

            if (hai == null)
            {
                hai = new User { Id = new Guid("9E18BB68-66D2-4711-A27B-1A54AC2E8077"), UserName = "hai" };
                await this.userRepository.CreateAsync(hai, "q");
            }

            var english = new Language()
            {
                Name = "English",
                Id = Guid.NewGuid(),
                CreatorId = hai.Id,
                DelimiterPattern = @"[ ]",
            };

            var mandarin = new Language()
            {
                Name = "Mandarin",
                Id = Guid.NewGuid(),
                CreatorId = hai.Id,
                DelimiterPattern = string.Empty,
            };

            this.languageRepository.AddAsync(english).GetAwaiter().GetResult();
            this.languageRepository.AddAsync(mandarin).GetAwaiter().GetResult();
        }
    }
}