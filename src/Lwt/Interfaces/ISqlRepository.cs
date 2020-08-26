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
    public interface ISqlRepository<T>
        where T : Entity
    {
        /// <summary>
        /// insert an new entity to database.
        /// </summary>
        /// <param name="entity">entity.</param>
        void Add(T entity);

        /// <summary>
        /// insert many new entity to database.
        /// </summary>
        /// <param name="entities">entity.</param>
        void BulkAdd(IEnumerable<T> entities);

        /// <summary>
        /// delete an entity by id.
        /// </summary>
        /// <param name="entity">entity.</param>
        void DeleteById(T entity);

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
        void Update(T entity);

        /// <summary>
        /// get an entity by id if not found return null.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T?> TryGetByIdAsync(int id);

        /// <summary>
        /// get an entity by id if not found throw not found exception.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// check if an entity exist.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> IsExistAsync(int id);

        /// <summary>
        /// count all document in the collection.
        /// </summary>
        /// <param name="filter"> the filter.</param>
        /// <returns>the count.</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        /// <summary>
        /// delete an entity.
        /// </summary>
        /// <param name="entity">entity.</param>
        void Delete(T entity);

        void BulkInsert(IEnumerable<T> entities);
    }
}