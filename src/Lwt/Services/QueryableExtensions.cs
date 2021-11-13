namespace Lwt.Services;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

public static class QueryableExtensions
{
    public static string ToSql<TEntity>(this IQueryable<TEntity> query)
        where TEntity : class
    {
        using IEnumerator<TEntity> enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression)
            .GetEnumerator();
        object relationalCommandCache = enumerator.Private("_relationalCommandCache");
        SelectExpression selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
        IQuerySqlGeneratorFactory factory =
            relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

        QuerySqlGenerator sqlGenerator = factory.Create();
        IRelationalCommand command = sqlGenerator.GetCommand(selectExpression);

        string sql = command.CommandText;
        return sql;
    }

    private static object Private(this object obj, string privateField) =>
        obj.GetType()
            .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(obj) !;

    private static T Private<T>(this object obj, string privateField) =>
        (T)obj.GetType()
            .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(obj) !;
}