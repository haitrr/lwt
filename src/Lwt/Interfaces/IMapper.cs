namespace Lwt.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="TFrom">TFrom.</typeparam>
    /// <typeparam name="TTo">TTo.</typeparam>
    public interface IMapper<TFrom, TTo>
        where TTo : class
    {
        /// <summary>
        /// as.
        /// </summary>
        /// <param name="from">from.</param>
        /// <returns>a.</returns>
        TTo Map(TFrom from);

    /// <summary>
    /// a.
    /// </summary>
    /// <param name="from">from.</param>
    /// <param name="result">result.</param>
    /// <returns>asd.</returns>
        TTo Map(TFrom from, TTo result);

        /// <summary>
        /// asd.
        /// </summary>
        /// <param name="froms">froms.</param>
        /// <returns>zxc.</returns>
        ICollection<TTo> Map(IEnumerable<TFrom> froms);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="TFrom1">TFrom1.</typeparam>
    /// <typeparam name="TFrom2">TFrom2.</typeparam>
    /// <typeparam name="TTo">TTo.</typeparam>
    public interface IMapper<TFrom1, TFrom2, TTo>
    {
        /// <summary>
        /// zx.
        /// </summary>
        /// <param name="from1">from1.</param>
        /// <param name="from2">from2.</param>
        /// <returns>213.</returns>
        TTo Map(TFrom1 from1, TFrom2 from2);

        /// <summary>
        /// asd.
        /// </summary>
        /// <param name="from">from.</param>
        /// <param name="from2">from2.</param>
        /// <param name="result">to.</param>
        /// <returns>zx.</returns>
        TTo Map(TFrom1 from, TFrom2 from2, TTo result);

        /// <summary>
        /// d.
        /// </summary>
        /// <param name="from1S">from1s.</param>
        /// <param name="from2S">from2s.</param>
        /// <returns>a.</returns>
        ICollection<TTo> Map(IEnumerable<TFrom1> from1S, IEnumerable<TFrom2> from2S);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <typeparam name="TFrom1">TFrom1.</typeparam>
    /// <typeparam name="TFrom2">TFrom2.</typeparam>
    /// <typeparam name="TFrom3">TFrom3.</typeparam>
    /// <typeparam name="TTo">TTo.</typeparam>
    public interface IMapper<TFrom1, TFrom2, TFrom3, TTo>
    {
        /// <summary>
        /// z.
        /// </summary>
        /// <param name="from1">from1.</param>
        /// <param name="from2">from2.</param>
        /// <param name="from3">from3.</param>
        /// <returns>c.</returns>
        TTo Map(TFrom1 from1, TFrom2 from2, TFrom3 from3);

    /// <summary>
    /// a.
    /// </summary>
    /// <param name="from">from.</param>
    /// <param name="from2">from2.</param>
    /// <param name="from3">from3.</param>
    /// <param name="result">result.</param>
    /// <returns>d.</returns>
        TTo Map(TFrom1 from, TFrom2 from2, TFrom3 from3, TTo result);

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="from1S">from1s.</param>
        /// <param name="from2S">from2s.</param>
        /// <param name="from3S">from3s.</param>
        /// <returns>f.</returns>
        ICollection<TTo> Map(IEnumerable<TFrom1> from1S, IEnumerable<TFrom2> from2S, IEnumerable<TFrom3> from3S);
    }
}