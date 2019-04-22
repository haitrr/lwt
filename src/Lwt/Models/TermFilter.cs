namespace Lwt.Models
{
    using System;
    using System.Linq.Expressions;
    using Lwt.Extensions;
    using MongoDB.Driver;

    /// <summary>
    /// term's filters.
    /// </summary>
    public class TermFilter
    {
        /// <summary>
        /// Gets or sets term's language.
        /// </summary>
        public Language? Language { get; set; }

        /// <summary>
        /// get the term filter expression.
        /// </summary>
        /// <returns>the expression.</returns>
        public Expression<Func<Term, bool>> ToExpression()
        {
            Expression<Func<Term, bool>> expression = term => true;

            if (this.Language.HasValue)
            {
                expression = expression.And(term => (int)this.Language.Value == (int)term.Language);
            }

            return expression;
        }

        /// <summary>
        /// return filter definition.
        /// </summary>
        /// <returns>the filter definition.</returns>
        public FilterDefinition<Term> ToFilterDefinition()
        {
            FilterDefinitionBuilder<Term> filterBuilder = Builders<Term>.Filter;
            FilterDefinition<Term> filter = filterBuilder.Empty;

            if (this.Language.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(term => term.Language, this.Language.Value));
            }

            return filter;
        }
    }
}