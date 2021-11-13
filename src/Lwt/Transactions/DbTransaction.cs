namespace Lwt.Transactions;

using System.Threading.Tasks;
using Lwt.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// transaction.
/// </summary>
/// <typeparam name="T">type.</typeparam>
public class DbTransaction<T> : IDbTransaction
    where T : DbContext
{
    private readonly T databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbTransaction{T}"/> class.
    /// </summary>
    /// <param name="databaseContext">dbContext.</param>
    public DbTransaction(T databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc/>
    public async Task CommitAsync()
    {
        await this.databaseContext.SaveChangesAsync();
    }

    public IDbContextTransaction BeginTransaction()
    {
        return this.databaseContext.Database.BeginTransaction();
    }
}