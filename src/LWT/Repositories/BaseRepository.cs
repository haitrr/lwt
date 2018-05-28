using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.EntityFrameworkCore;

namespace Lwt.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly LwtDbContext LwtDbContext;
        protected readonly DbSet<T> DbSet;
        public BaseRepository(LwtDbContext lwtDbContext)
        {
            LwtDbContext = lwtDbContext;
            DbSet = LwtDbContext.Set<T>();
        }
        public void Add(T entity)
        {
            DbSet.Add(entity);
        }
    }
}
