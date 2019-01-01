namespace Lwt.Mappers
{
    using System;

    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// term create mapper.
    /// </summary>
    public class TermCreateMapper : BaseMapper<TermCreateModel, Guid, Term>
    {
        /// <inheritdoc/>
        public override Term Map(TermCreateModel createModel, Guid userId, Term term)
        {
            term.Content = createModel.Content.ToUpperInvariant();
            term.Meaning = createModel.Meaning;
            term.LearningLevel = createModel.LearningLevel;
            term.CreatorId = userId;

            return term;
        }
    }
}