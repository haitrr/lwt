namespace Lwt.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;

    using Microsoft.EntityFrameworkCore;

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

        /// <inheritdoc/>
        public async Task<ICollection<Language>> GetByUserAsync(Guid userId)
        {
            return await this.DbSet.Where(language => language.CreatorId == userId).ToListAsync();
        }
    }
}