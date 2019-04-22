namespace Lwt.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

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
        protected BaseRepository(LwtDbContext lwtDbContext)
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

        /// <inheritdoc />
        public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> filter, PaginationQuery paginationQuery)
        {
            return await this.Collection.Find(filter).Skip((paginationQuery.Page - 1) * paginationQuery.ItemPerPage)
                .Limit(paginationQuery.ItemPerPage).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> SearchAsync(FilterDefinition<T> filter, PaginationQuery paginationQuery)
        {
            return await this.Collection.Find(filter).Skip((paginationQuery.Page - 1) * paginationQuery.ItemPerPage)
                .Limit(paginationQuery.ItemPerPage).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(T entity)
        {
            entity.LastModifiedDate = DateTime.Now;
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
        public async Task<long> CountAsync(Expression<Func<T, bool>> filter = null)
        {
            return await this.Collection.AsQueryable().Where(filter ?? (_ => true)).LongCountAsync();
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(FilterDefinition<T> filter)
        {
            return await this.Collection.CountDocumentsAsync(filter ?? Builders<T>.Filter.Empty);
        }
    }
}