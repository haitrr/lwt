using Lwt.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<bool> Commit()
        {
            int rowsChanged = await _dbContext.SaveChangesAsync();
            return rowsChanged > 0;
        }
    }
}
