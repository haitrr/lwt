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
            await this.CreateTermIndexesAsync();
            await this.CreateTextIndexesAsync();
        }

        private async Task CreateTermIndexesAsync()
        {
            IMongoCollection<Term> collection = this.lwtDbContext.Terms;
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Term>(
                Builders<Term>.IndexKeys.Ascending(term => term.Content)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Term>(
                Builders<Term>.IndexKeys.Ascending(term => term.CreatorId)));
        }

        private async Task CreateTextIndexesAsync()
        {
            IMongoCollection<Text> collection = this.lwtDbContext.Texts;
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Text>(
                Builders<Text>.IndexKeys.Ascending(text => text.Language)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Text>(
                Builders<Text>.IndexKeys.Ascending(text => text.CreatorId)));
        }
    }
}