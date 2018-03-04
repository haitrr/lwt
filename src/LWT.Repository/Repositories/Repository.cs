using System;
using System.Linq;
using LWT.Repository.Interfaces;
using LWT.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LWT.Repository.Repositories
{
    public class BaseRepository<TEntity> : IDisposable,IRepository<TEntity> where TEntity : class
    {
        protected readonly LWTDbContext context;
        protected readonly DbSet<TEntity> dbSet;

        public BaseRepository(LWTDbContext context)
        {
            this.context = context;
            dbSet = this.context.Set<TEntity>();
        }

        public virtual void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual TEntity GetById(Guid id)
        {
            return dbSet.Find(id);
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return dbSet;
        }

        public virtual void Update(TEntity entity)
        {
            dbSet.Update(entity);
        }

        public virtual void Remove(Guid id)
        {
            dbSet.Remove(dbSet.Find(id));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                this.context.Dispose();
            }
        }

    }
}