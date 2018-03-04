using System;
using LWT.Data.Contexts;
using LWT.Repository.Interfaces;

namespace LWT.Repository.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LWTDbContext _context;

        public UnitOfWork(LWTDbContext context)
        {
            _context = context;
        }

        public int Commit()
        {
            int rowsAffected = _context.SaveChanges();
            return rowsAffected;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
        }
    }
}