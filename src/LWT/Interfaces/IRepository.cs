using Lwt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lwt.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        void Add(T entity);
    }
}
