namespace Lwt.Repositories
{
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc cref="ILanguageRepository" />
    public class LanguageRepository : BaseRepository<Language>, ILanguageRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageRepository"/> class.
        /// </summary>
        /// <param name="lwtDbContext">the db context.</param>
        public LanguageRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }
    }
}