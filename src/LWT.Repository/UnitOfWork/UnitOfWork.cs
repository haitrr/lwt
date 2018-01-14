using LWT.Repository.Interfaces;
using LWT.Data.Contexts;

namespace LWT.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LWTContext _context;

        public UnitOfWork(LWTContext context)
        {
            _context = context;
        }

        public int Commit()
        {
            var rowsAffected = _context.SaveChanges();
            return rowsAffected;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}