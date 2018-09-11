using LWT.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces.Services
{
    public interface ITextService
    {
        Task CreateAsync(Guid userId, Text text);
        Task<IEnumerable<Text>> GetByUserAsync(Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
        Task EditAsync(Guid textId, Guid userId, TextEditModel editModel);
    }
}