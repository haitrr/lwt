using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LWT.Models;

namespace Lwt.Interfaces
{
    public interface ITextRepository : IRepository<Text>
    {
        Task<IEnumerable<Text>> GetByUserAsync(Guid userId);
    }
}
