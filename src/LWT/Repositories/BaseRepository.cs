namespace Lwt.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    public abstract class BaseRepository<T> : IRepository<T>
        where T : Entity
    {
        /// <summary>
        /// a.
        /// </summary>
        private readonly LwtDbContext lwtDbContext;

        /// <summary>
        /// a.
        /// </summary>
        private readonly DbSet<T> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
        /// </summary>
        /// <param name="lwtDbContext">a.</param>
        public BaseRepository(LwtDbContext lwtDbContext)
        {
            this.lwtDbContext = lwtDbContext;
            this.dbSet = this.lwtDbContext.Set<T>();
        }

        /// <summary>
        /// Gets contex.
        /// </summary>
        protected LwtDbContext LwtDbContext => lwtDbContext;

        /// <summary>
        /// gets dbset.
        /// </summary>
        protected DbSet<T> DbSet => dbSet;

        /// <inheritdoc/>
        public void Add(T entity)
        {
            this.DbSet.Add(entity);
        }

        /// <inheritdoc/>
        public void DeleteById(T entity)
        {
            this.DbSet.Remove(entity);
        }

        /// <inheritdoc/>
        public void Update(T entity)
        {
            this.DbSet.Update(entity);
        }

        /// <inheritdoc/>
        public Task<T> GetByIdAsync(Guid id)
        {
            try
            {
                return this.DbSet.SingleAsync(ad => ad.Id == id);
            }
            catch
            {
                throw new NotFoundException($"Item with id {id} not found.");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsExists(Guid id)
        {
            return await this.DbSet.FindAsync(id) != null;
        }
    }
}