using LWT.Repository.Interfaces;
using LWT.Data.Contexts;
using System;

namespace LWT.Repository.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly LWTContext context;

        public UnitOfWork(LWTContext context)
        {
            this.context = context;
        }

        public int Commit()
        {
            var rowsAffected = context.SaveChanges();
            return rowsAffected;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.context.Dispose();
            }
        }
    }
}