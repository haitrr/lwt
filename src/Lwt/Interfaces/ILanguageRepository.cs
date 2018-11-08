namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Models;

    /// <summary>
    /// s.
    /// </summary>
    public interface ILanguageRepository : IRepository<Language>
    {
        /// <summary>
        /// return languages that create by the user.
        /// </summary>
        /// <param name="userId">the user id.</param>
        /// <returns>language that is created by the user.</returns>
        Task<ICollection<Language>> GetByUserAsync(Guid userId);
    }
}