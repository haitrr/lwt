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
        private readonly T databaseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction{T}"/> class.
        /// </summary>
        /// <param name="databaseContext">dbContext.</param>
        public Transaction(T databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        /// <inheritdoc/>
        public async Task Commit()
        {
            await this.databaseContext.SaveChangesAsync();
        }
    }
}