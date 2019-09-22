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
        public override Term Map(TermCreateModel from, Guid from2, Term result)
        {
            result.Content = from.Content.ToUpperInvariant();
            result.Meaning = from.Meaning;
            result.LearningLevel = from.LearningLevel;
            result.Language = from.Language;
            result.CreatorId = from2;

            return result;
        }
    }
}