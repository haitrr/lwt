namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LWT.Models;

    /// <summary>
    /// a.
    /// </summary>
    public interface ITextRepository : IRepository<Text>
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<Text>> GetByUserAsync(Guid userId);
    }
}
