namespace Lwt.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// a.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Commit();
    }
}