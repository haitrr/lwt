namespace Lwt.Mappers
{
    using System.Threading.Tasks;

    /// <inheritdoc/>
    public abstract class AbstractAsyncMapper<TSource, TResult> : IAsyncMapper<TSource, TResult>
        where TResult : class, new()
    {
        /// <inheritdoc/>
        public abstract Task<TResult> MapAsync(TSource source, TResult result);

        /// <inheritdoc/>
        public Task<TResult> MapAsync(TSource source)
        {
            var result = new TResult();
            return this.MapAsync(source, result);
        }
    }
}