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

    /// <summary>
    /// a.
    /// </summary>
    public class TextRepository : BaseRepository<Text>, ITextRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextRepository"/> class.
        /// </summary>
        /// <param name="lwtDbContext">lwtDbContext.</param>
        public TextRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Text>> GetByUserAsync(
            Guid userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery)
        {
            int skip = paginationQuery.ItemPerPage * (paginationQuery.Page - 1);

            return await this.Filter(userId, textFilter).Skip(skip).Take(paginationQuery.ItemPerPage)
                .Include(text => text.Language).ToListAsync();
        }

        /// <inheritdoc/>
        public Task<int> CountByUserAsync(Guid userId, TextFilter textFilter)
        {
            return this.Filter(userId, textFilter).CountAsync();
        }

        private IQueryable<Text> Filter(Guid userId, TextFilter textFilter)
        {
            IQueryable<Text> texts = this.DbSet.Where(t => t.CreatorId == userId);

            if (textFilter.LanguageId != null)
            {
                texts = texts.Where(t => t.LanguageId == textFilter.LanguageId);
            }

            return texts;
        }
    }
}