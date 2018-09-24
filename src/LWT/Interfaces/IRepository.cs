namespace Lwt.Interfaces
{
    using System;
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
        /// a.
        /// </summary>
        /// <param name="entity">entity.</param>
        void Add(T entity);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="entity">entity.</param>
        void DeleteById(T entity);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="entity">entity.</param>
        void Update(T entity);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> IsExists(Guid id);
    }
}