using LWT.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lwt.Interfaces.Services
{
    public interface ITextService
    {
        Task<bool> CreateAsync(Guid userId,Text text);
        Task<IEnumerable<Text>> GetByUserAsync(Guid userId);
    }
}
