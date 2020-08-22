namespace Lwt.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;

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

            // sort by created time by default
            return await this.Filter(userId, textFilter)
                .Sort(Builders<Text>.Sort.Descending(text => text.CreatedDate))
                .Skip(skip)
                .Limit(paginationQuery.ItemPerPage)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public Task<long> CountByUserAsync(Guid userId, TextFilter textFilter)
        {
            return this.Filter(userId, textFilter).CountDocumentsAsync();
        }

        private IFindFluent<Text, Text> Filter(Guid userId, TextFilter textFilter)
        {
            FilterDefinitionBuilder<Text> builder = Builders<Text>.Filter;
            FilterDefinition<Text> filter = builder.Eq(t => t.CreatorId, userId);

            if (textFilter.LanguageCode != null)
            {
                filter = builder.And(filter, builder.Eq(t => t.LanguageCode, textFilter.LanguageCode));
            }

            return this.Collection.Find(filter);
        }
    }
}