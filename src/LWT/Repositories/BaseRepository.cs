using System;
using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Exceptions;
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

        public void DeleteById(T entity)
        {
            DbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            DbSet.Update(entity);
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            try
            {
                return DbSet.SingleAsync(ad => ad.Id == id);
            }
            catch
            {
                throw new NotFoundException($"Item with id {id} not found.");
            }
        }

        public async Task<bool> IsExists(Guid id)
        {
            return await DbSet.FindAsync(id.ToString()) != null;
        }
    }
}