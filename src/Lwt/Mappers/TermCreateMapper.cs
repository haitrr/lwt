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
        public override Term Map(TermCreateModel from1, Guid from2, Term result)
        {
            result.Content = from1.Content.ToUpperInvariant();
            result.Meaning = from1.Meaning;
            result.LearningLevel = from1.LearningLevel;
            result.Language = from1.Language;
            result.CreatorId = from2;

            return result;
        }
    }
}