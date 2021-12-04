namespace Lwt.Interfaces;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// db transaction.
/// </summary>
public interface IDbTransaction
{
    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task CommitAsync();

    IDbContextTransaction BeginTransaction();
    void SaveChanges();
}