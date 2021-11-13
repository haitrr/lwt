namespace Lwt.Mappers;

using System.Threading.Tasks;

/// <summary>
/// an async mapper.
/// </summary>
/// <typeparam name="TSource">mapping source type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public interface IAsyncMapper<TSource, TResult>
    where TResult : class, new()
{
    /// <summary>
    /// map into existing result object.
    /// </summary>
    /// <param name="source">the source object.</param>
    /// <param name="result">result object.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<TResult> MapAsync(TSource source, TResult result);

    /// <summary>
    /// create, map and return result.
    /// </summary>
    /// <param name="source">the source.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<TResult> MapAsync(TSource source);
}