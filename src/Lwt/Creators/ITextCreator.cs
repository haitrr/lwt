namespace Lwt.Creators
{
    using System.Threading.Tasks;
    using Lwt.Models;

    /// <summary>
    /// text creator.
    /// </summary>
    public interface ITextCreator
    {
        /// <summary>
        /// create a text asynchronously.
        /// </summary>
        /// <param name="text">the text to create.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<int> CreateAsync(Text text);
    }
}