using Lwt.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Lwt.Transactions
{
    public class Transaction<T> : ITransaction where T : DbContext
    {
        private readonly T _dbContext;

        public Transaction(T dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Commit()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}