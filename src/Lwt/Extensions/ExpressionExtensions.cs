namespace Lwt.Extensions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// expression extensions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// and operator.
        /// </summary>
        /// <param name="current">current expression.</param>
        /// <param name="expression">the other expression.</param>
        /// <typeparam name="T">type of entity.</typeparam>
        /// <returns>the result expression.</returns>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> current,
            Expression<Func<T, bool>> expression)
        {
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(current.Body, expression.Body),
                current.Parameters[0]);
        }
    }
}