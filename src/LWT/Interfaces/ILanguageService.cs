using System;
using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces
{
    public interface ILanguageService
    {
        Task<Guid> CreateAsync(Guid creatorId, LanguageCreateModel languageCreateModel);
    }
}