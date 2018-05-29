using Lwt.Models;
namespace Lwt.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        void Add(T entity);
    }
}
