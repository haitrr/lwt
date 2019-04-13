using System;
using System.Linq.Expressions;
using Lucene.Net.Analysis;

namespace Lwt.Models
{
    /// <summary>
    /// term's filters.
    /// </summary>
    public class TermFilter
    {
        /// <summary>
        /// Gets or sets term's language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// get the term filter expression.
        /// </summary>
        /// <returns>the expression.</returns>
        public virtual Expression<Func<Term, bool>> ToExpression()
        {
            return term => term.Language == this.Language;
        }
    }
}