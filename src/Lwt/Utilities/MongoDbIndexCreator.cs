namespace Lwt.Utilities
{
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;

    /// <inheritdoc />
    public class MongoDbIndexCreator : IIndexCreator
    {
        private readonly LwtDbContext lwtDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIndexCreator"/> class.
        /// </summary>
        /// <param name="lwtDbContext">the index db context.</param>
        public MongoDbIndexCreator(LwtDbContext lwtDbContext)
        {
            this.lwtDbContext = lwtDbContext;
        }

        /// <inheritdoc />
        public async Task CreateIndexesAsync()
        {
            await this.CreateTextIndexesAsync();
        }

        private async Task CreateTextIndexesAsync()
        {
            IMongoCollection<Text> collection = this.lwtDbContext.GetCollection<Text>();
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Text>(
                Builders<Text>.IndexKeys.Ascending(text => text.LanguageCode)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Text>(
                Builders<Text>.IndexKeys.Ascending(text => text.CreatorId)));
        }
    }
}