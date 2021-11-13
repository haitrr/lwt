namespace Lwt.Models;

using System;
using System.Linq.Expressions;
using Lwt.Extensions;

/// <summary>
/// term's filters.
/// </summary>
public class TermFilter
{
    /// <summary>
    /// Gets or sets term's language.
    /// </summary>
    public LanguageCode? LanguageCode { get; set; }

    /// <summary>
    /// get the term filter expression.
    /// </summary>
    /// <returns>the expression.</returns>
    public Expression<Func<Term, bool>> ToExpression()
    {
        Expression<Func<Term, bool>> expression = term => true;

        if (this.LanguageCode != null)
        {
            expression = expression.And(term => this.LanguageCode == term.LanguageCode);
        }

        return expression;
    }
}