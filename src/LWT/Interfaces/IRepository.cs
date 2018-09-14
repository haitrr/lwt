using System;
using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        void Add(T entity);
        void DeleteById(T entity);
        void Update(T entity);
        Task<T> GetByIdAsync(Guid id);
        Task<bool> IsExists(Guid id);
    }
}