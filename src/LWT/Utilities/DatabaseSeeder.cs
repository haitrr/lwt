namespace Lwt.Utilities
{
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="userRepository"> user repo.</param>
        public DatabaseSeeder(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <inheritdoc />
        public void SeedData()
        {
            var hai = new User {UserName = "hai"};
            this.userRepository.CreateAsync(hai, "q");
        }
    }
}