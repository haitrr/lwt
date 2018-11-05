namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public interface ITermService
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <param name="term">term.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> CreateAsync(Term term);
    }
}