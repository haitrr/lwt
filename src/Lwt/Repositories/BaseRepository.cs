namespace Lwt.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;

    /// <summary>
    /// the base repository.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    public abstract class BaseRepository<T> : IRepository<T>
        where T : Entity
    {
        /// <summary>
        /// the db context.
        /// </summary>
        private readonly LwtDbContext lwtDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
        /// </summary>
        /// <param name="lwtDbContext">a.</param>
        public BaseRepository(LwtDbContext lwtDbContext)
        {
            this.lwtDbContext = lwtDbContext;
            this.Collection = this.lwtDbContext.GetCollection<T>();
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        protected IMongoCollection<T> Collection { get; }

        /// <inheritdoc/>
        public virtual async Task AddAsync(T entity)
        {
            await this.Collection.InsertOneAsync(entity);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByIdAsync(T entity)
        {
            DeleteResult result = await this.Collection.DeleteOneAsync(e => e.Id == entity.Id);

            if (result.DeletedCount == 0)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(T entity)
        {
            ReplaceOneResult result = await this.Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);

            if (result.ModifiedCount == 0)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public Task<T> GetByIdAsync(Guid id)
        {
            return this.Collection.Find(e => e.Id == id).SingleOrDefaultAsync();
        }

        /// <inheritdoc/>
        public Task<bool> IsExistAsync(Guid id)
        {
            return this.Collection.Find(e => e.Id == id).AnyAsync();
        }

        /// <inheritdoc />
        public Task<long> CountAsync()
        {
            return this.Collection.CountDocumentsAsync(_ => true);
        }
    }
}