namespace Lwt.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc cref="Lwt.Repositories.ISqlTextRepository" />
    public class SqlTextRepository : BaseSqlRepository<Text>, ISqlTextRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTextRepository"/> class.
        /// </summary>
        /// <param name="identityDbContext">db context.</param>
        public SqlTextRepository(IdentityDbContext identityDbContext)
            : base(identityDbContext)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Text>> GetByUserAsync(
            int userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery)
        {
            int skip = paginationQuery.ItemPerPage * (paginationQuery.Page - 1);

            // sort by created time by default
            if (textFilter.LanguageCode != null)
            {
                return await this.DbSet.Where(t => t.UserId == userId && t.LanguageCode == textFilter.LanguageCode)
                    .ToListAsync();
            }

            return await this.DbSet.Where(t => t.UserId == userId)
                .Skip(skip)
                .Take(paginationQuery.ItemPerPage)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<long> CountByUserAsync(int userId, TextFilter textFilter)
        {
            // sort by created time by default
            if (textFilter.LanguageCode != null)
            {
                return await this.DbSet.Where(t => t.LanguageCode == textFilter.LanguageCode && t.UserId == userId)
                    .CountAsync();
            }

            return await this.DbSet.Where(t => t.UserId == userId)
                .CountAsync();
        }
    }
}