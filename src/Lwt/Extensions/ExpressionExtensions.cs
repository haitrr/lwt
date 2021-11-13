namespace Lwt.Extensions;

using System;
using System.Linq.Expressions;

/// <summary>
/// expression extensions.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// true expression.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    /// <returns>return true expression.</returns>
    public static Expression<Func<T, bool>> True<T>()
    {
        return f => true;
    }

    /// <summary>
    /// false expression.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    /// <returns>return false expression.</returns>
    public static Expression<Func<T, bool>> False<T>()
    {
        return f => false;
    }

    /// <summary>
    /// get the result from or two expression.
    /// </summary>
    /// <param name="expr1"> expression 1.</param>
    /// <param name="expr2">expression 2.</param>
    /// <typeparam name="T">type.</typeparam>
    /// <returns>result expression from or two inputs.</returns>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        InvocationExpression invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// get result from and two expression.
    /// </summary>
    /// <param name="expr1">expression 1.</param>
    /// <param name="expr2">expression 2.</param>
    /// <typeparam name="T">type.</typeparam>
    /// <returns>result expression from and two inputs.</returns>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        InvocationExpression invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
}