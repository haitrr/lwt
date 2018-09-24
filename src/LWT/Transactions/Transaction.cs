namespace Lwt.Transactions
{
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// transaction.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    public class Transaction<T> : ITransaction
        where T : DbContext
    {
        private readonly T dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction{T}"/> class.
        /// </summary>
        /// <param name="dbContext">dbContext.</param>
        public Transaction(T dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task Commit()
        {
            await this.dbContext.SaveChangesAsync();
        }
    }
}