namespace Lwt.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// database index creator.
    /// </summary>
    public interface IIndexCreator
    {
        /// <summary>
        /// create database indexes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateIndexesAsync();
    }
}