using System;
using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces
{
    public interface ITermService
    {
        Task<Guid> CreateAsync(Term term);
    }
}