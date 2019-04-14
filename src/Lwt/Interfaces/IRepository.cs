namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    public interface IRepository<T>
        where T : Entity
    {
        /// <summary>
        /// insert an new entity to database.
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// delete an entity by id.
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<bool> DeleteByIdAsync(T entity);

        /// <summary>
        /// Search for entities.
        /// </summary>
        /// <param name="filter">the filter.</param>
        /// <param name="paginationQuery">pagination query.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> filter, PaginationQuery paginationQuery);

        /// <summary>
        /// update an entity.
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// get an entity by id.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// check if an entity exist.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> IsExistAsync(Guid id);

        /// <summary>
        /// count all document in the collection.
        /// </summary>
        /// <param name="filter"> the filter.</param>
        /// <returns>the count.</returns>
        Task<ulong> CountAsync(Expression<Func<T, bool>> filter = null);
    }
}