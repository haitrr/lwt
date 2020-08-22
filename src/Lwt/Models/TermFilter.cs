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

        /// <summary>
        /// return filter definition.
        /// </summary>
        /// <returns>the filter definition.</returns>
        public FilterDefinition<Term> ToFilterDefinition()
        {
            FilterDefinitionBuilder<Term> filterBuilder = Builders<Term>.Filter;
            FilterDefinition<Term> filter = filterBuilder.Empty;

            if (this.LanguageCode != null)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(term => term.LanguageCode, this.LanguageCode));
            }

            return filter;
        }
    }
}