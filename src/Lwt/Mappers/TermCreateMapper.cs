namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// term create mapper.
    /// </summary>
    public class TermCreateMapper : BaseMapper<TermCreateModel, int, Term>
    {
        /// <inheritdoc/>
        public override Term Map(TermCreateModel from, int from2, Term result)
        {
            result.Content = from.Content.ToUpperInvariant();
            result.Meaning = from.Meaning;
            result.LearningLevel = from.LearningLevel;
            result.LanguageCode = from.LanguageCode;
            result.UserId = from2;

            return result;
        }
    }
}