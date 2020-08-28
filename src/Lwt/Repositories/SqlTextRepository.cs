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

            IQueryable<Text> query = this.DbSet;

            // sort by created time by default
            if (textFilter.LanguageCode != null)
            {
                query = query.Where(t => t.UserId == userId && t.LanguageCode == textFilter.LanguageCode);
            }

            return await query
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Select(t => new Text
                {
                    Id = t.Id,
                    Title = t.Title,
                    LanguageCode = t.LanguageCode,
                })
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