namespace Lwt.Interfaces
{
    using System.Threading.Tasks;

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
    }
}